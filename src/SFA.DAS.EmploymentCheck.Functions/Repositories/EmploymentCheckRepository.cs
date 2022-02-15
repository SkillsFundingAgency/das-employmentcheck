using Ardalis.GuardClauses;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DbConnection = SFA.DAS.EmploymentCheck.Functions.Application.Helpers.DbConnection;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class EmploymentCheckRepository : IEmploymentCheckRepository
    {
        private readonly ILogger<EmploymentCheckRepository> _logger;
        private readonly ApplicationSettings _settings;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckRepository(
            ApplicationSettings applicationSettings,
            ILogger<EmploymentCheckRepository> logger,
            AzureServiceTokenProvider azureServiceTokenProvider = null
        )
        {
            _logger = logger;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _settings = applicationSettings;
        }

        public async Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            IList<Models.EmploymentCheck> employmentChecksBatch;

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                             _settings.DbConnectionString,
                _azureServiceTokenProvider))
            {
                await sqlConnection.OpenAsync();
                {
                    var transaction = sqlConnection.BeginTransaction();

                    try
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@batchSize", _settings.BatchSize);

                        employmentChecksBatch = (await sqlConnection.QueryAsync<Models.EmploymentCheck>(
                                sql: "SELECT TOP (@batchSize) " +
                                    "[Id], " +
                                    "[CorrelationId], " +
                                    "[CheckType], " +
                                    "[Uln], " +
                                    "[ApprenticeshipId], " +
                                    "[AccountId], " +
                                    "[MinDate], " +
                                    "[MaxDate], " +
                                    "[Employed], " +
                                    "[RequestCompletionStatus], " +
                                    "[CreatedOn], " +
                                    "[LastUpdatedOn] " +
                                    "FROM [Business].[EmploymentCheck] AEC " +
                                    "WHERE (AEC.RequestCompletionStatus IS NULL) " +
                                    "ORDER BY AEC.Id ",
                                param: parameters,
                                commandType: CommandType.Text,
                                transaction: transaction)).ToList();

                        if (employmentChecksBatch.Count > 0)
                        {
                            foreach (var employmentCheck in employmentChecksBatch)
                            {
                                var parameter = new DynamicParameters();
                                parameter.Add("@Id", employmentCheck.Id, DbType.Int64);
                                parameter.Add("@requestCompletionStatus", ProcessingCompletionStatus.Started, DbType.Int16);
                                parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                                await sqlConnection.ExecuteAsync(
                                    "UPDATE [Business].[EmploymentCheck] " +
                                    "SET RequestCompletionStatus = @requestCompletionStatus, LastUpdatedOn = @lastUpdatedOn " +
                                    "WHERE Id = @Id ",
                                    parameter,
                                    commandType: CommandType.Text,
                                    transaction: transaction);
                            }

                            transaction.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        _logger.LogError($"EmploymentCheckService.GetEmploymentChecksBatch(): ERROR: An error occurred reading the employment checks. Exception [{e}]");
                        throw;
                    }
                }
            }

            return employmentChecksBatch;
        }

        public async Task InsertOrUpdate(Models.EmploymentCheck check)
        {
            Guard.Against.Null(check, nameof(check));

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _settings.DbConnectionString,
                _azureServiceTokenProvider)
            )
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));
                await sqlConnection.OpenAsync();
                using (var tran = await sqlConnection.BeginTransactionAsync())
                {
                    try
                    {
                        var existingItem = await sqlConnection.GetAsync<Models.EmploymentCheck>(check.Id, tran);
                        if (existingItem != null)
                        {
                            check.LastUpdatedOn = DateTime.Now;
                            await sqlConnection.UpdateAsync(check, tran);
                        }
                        else
                        {
                            try
                            {
                                check.CreatedOn = DateTime.Now;
                                await sqlConnection.InsertAsync(check, tran);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"{nameof(AccountsResponseRepository)} Exception caught: {ex.Message}. {ex.StackTrace}");
                                throw;
                            }
                        }

                        await tran.CommitAsync();
                    }
                    catch
                    {
                        await tran.RollbackAsync();
                    }
                }
            }
        }

        public async Task UpdateEmploymentCheckAsComplete(Models.EmploymentCheckCacheRequest request, IUnitOfWork unitOfWork)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@apprenticeEmploymentCheckId", request.ApprenticeEmploymentCheckId, DbType.Int64);
            parameter.Add("@employed", request.Employed, DbType.Boolean);
            parameter.Add("@requestCompletionStatus", (short)ProcessingCompletionStatus.Completed, DbType.Int16);
            parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

            const string sql = "UPDATE [Business].[EmploymentCheck] " +
                               "SET Employed = @employed, RequestCompletionStatus = @requestCompletionStatus, LastUpdatedOn = @lastUpdatedOn " +
                               "WHERE Id = @ApprenticeEmploymentCheckId AND (Employed IS NULL OR Employed = 0) ";
         
            await unitOfWork.ExecuteSqlAsync(sql, parameter);
        }
    }
}