using Ardalis.GuardClauses;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public class EmploymentCheckCacheRequestRepository : IEmploymentCheckCacheRequestRepository
    {
        private readonly IHmrcApiOptionsRepository _hmrcApiOptionsRepository;
        private readonly ILogger<EmploymentCheckCacheRequestRepository> _logger;
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckCacheRequestRepository(
            ApplicationSettings applicationSettings,
            IHmrcApiOptionsRepository hmrcApiOptionsRepository,
            ILogger<EmploymentCheckCacheRequestRepository> logger,
            AzureServiceTokenProvider azureServiceTokenProvider = null
        )
        {
            _hmrcApiOptionsRepository = hmrcApiOptionsRepository;
            _logger = logger;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task Save(EmploymentCheckCacheRequest request)
        {
            Guard.Against.Null(request, nameof(request));

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));
                await sqlConnection.OpenAsync();
                using (var tran = await sqlConnection.BeginTransactionAsync())
                {
                    try
                    {
                        var existingItem = await sqlConnection.GetAsync<EmploymentCheckCacheRequest>(request.Id, tran);
                        if (existingItem != null)
                        {
                            request.LastUpdatedOn = DateTime.Now;
                            await sqlConnection.UpdateAsync(request, tran);
                        }
                        else
                        {
                            request.LastUpdatedOn = null;
                            request.CreatedOn = DateTime.Now;
                            await sqlConnection.InsertAsync(request, tran);
                        }

                        await tran.CommitAsync();
                    }
                    catch(Exception ex)
                    {
                        await tran.RollbackAsync();
                        _logger.LogError($"{nameof(AccountsResponseRepository)} Exception caught: {ex.Message}. {ex.StackTrace}");
                    }
                }
            }
        }

        public async Task Insert(EmploymentCheckCacheRequest request)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.InsertAsync(request);
        }

        /// <summary>
        /// 'Related' requests are requests that have the same 'parent' EmploymentCheck
        /// (i.e. the same ApprenticeEmploymentCheckId, which is the foreign key from the EmploymentCheck table)
        /// </summary>
        public async Task AbandonRelatedRequests(EmploymentCheckCacheRequest request, IUnitOfWork unitOfWork)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.Id, DbType.Int64);
            parameters.Add("@ApprenticeEmploymentCheckId", request.ApprenticeEmploymentCheckId, DbType.Int64);
            parameters.Add("@nino", request.Nino, DbType.String);
            parameters.Add("@minDate", request.MinDate, DbType.DateTime);
            parameters.Add("@maxDate", request.MaxDate, DbType.DateTime);
            parameters.Add("@requestCompletionStatus", ProcessingCompletionStatus.Skipped, DbType.Int16);
            parameters.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

            const string sql =
                @"UPDATE [Cache].[EmploymentCheckCacheRequest]
                  SET    RequestCompletionStatus     =  @requestCompletionStatus,
                         Employed                    =  null,
                         LastUpdatedOn               =  @lastUpdatedOn
                  WHERE  Id                          <> @Id
                  AND    ApprenticeEmploymentCheckId =  @apprenticeEmploymentCheckId
                  AND    Nino                        =  @nino
                  AND    MinDate                     =  @minDate
                  AND    MaxDate                     =  @maxDate
                  AND    (Employed                   IS NULL OR Employed = 0)
                  AND    RequestCompletionStatus     IS NULL
                ";

            await unitOfWork.ExecuteSqlAsync(sql, parameters);
        }

        public async Task<EmploymentCheckCacheRequest[]> GetEmploymentCheckCacheRequests()
        {
            var rateLimiterOptions = await _hmrcApiOptionsRepository.GetHmrcRateLimiterOptions();
            var dbConnection = new DbConnection();

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            EmploymentCheckCacheRequest[] employmentCheckCacheRequests;
            await sqlConnection.OpenAsync();
            var transaction = sqlConnection.BeginTransaction();
            try
            {
                const string selectQuery = @"
                    ;WITH SmallestLearner AS
                    (
                        SELECT TOP(@employmentCheckBatchSize)
                            [ApprenticeEmploymentCheckId], --Group by ApprenticeEmploymentCheck
                            MIN([Id]) [RequestId],         --pick the oldes ID from each group of ApprenticeEmploymentCheck
                            COUNT([Id]) [Count]            --count number of checks in each group for ordering
                        FROM [Cache].[EmploymentCheckCacheRequest]
                        WHERE [RequestCompletionStatus] IS NULL
                        GROUP BY [ApprenticeEmploymentCheckId]
                        ORDER BY COUNT([Id]) ASC
                    )
                    SELECT TOP(@employmentCheckBatchSize)
                          r.[Id]
                        , r.[ApprenticeEmploymentCheckId]
                        , r.[CorrelationId] 
                        , r.[Nino]
                        , r.[PayeScheme]
                        , r.[MinDate]
                        , r.[MaxDate]
                        , r.[Employed]
                        , r.[RequestCompletionStatus]
                        , r.[CreatedOn]
                        , r.[LastUpdatedOn]
                    FROM [Cache].[EmploymentCheckCacheRequest] r
                    INNER JOIN SmallestLearner a
                    ON a.RequestId = r.Id --this does have a downside, if the batch size is 30 but only 15 learner remains then due to this join we will only process 15 learners in parallel
                    WHERE r.[RequestCompletionStatus] IS NULL
                    ;
                ";
                var selectParameter = new DynamicParameters();
                selectParameter.Add("@employmentCheckBatchSize", rateLimiterOptions.EmploymentCheckBatchSize, DbType.Int64);

                employmentCheckCacheRequests = (await sqlConnection.QueryAsync<EmploymentCheckCacheRequest>(
                    sql: selectQuery,
                    param: selectParameter,
                    commandType: CommandType.Text,
                    transaction: transaction)).ToArray();

                if (employmentCheckCacheRequests.Any())
                {
                    var updateParameter = new DynamicParameters();
                    updateParameter.Add("@Ids", employmentCheckCacheRequests.Select(ecr => ecr.Id).ToArray());
                    updateParameter.Add("@requestCompletionStatus", ProcessingCompletionStatus.Started, DbType.Int16);
                    updateParameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                    const string updateQuery = @"
                    UPDATE [Cache].[EmploymentCheckCacheRequest]
                    SET    RequestCompletionStatus = @requestCompletionStatus,
                           LastUpdatedOn = @lastUpdatedOn
                    WHERE  Id in @Ids";

                    await sqlConnection.ExecuteAsync(
                        sql: updateQuery,
                        param: updateParameter,
                        commandType: CommandType.Text,
                        transaction: transaction);
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            return employmentCheckCacheRequests;
        }
    }
}