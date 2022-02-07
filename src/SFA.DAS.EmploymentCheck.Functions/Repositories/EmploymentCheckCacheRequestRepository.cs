using Ardalis.GuardClauses;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class EmploymentCheckCacheRequestRepository
        : IEmploymentCheckCacheRequestRepository
    {
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckCacheRequestRepository(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null)
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task Save(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
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
                    // A try/catch block is needed here to intercept any exceptions
                    // from the database to rollback the transaction before passing
                    // the exception up the call stack
                    try
                    {
                        var existingItem = await sqlConnection.GetAsync<EmploymentCheckCacheRequest>(employmentCheckCacheRequest.Id, tran);
                        if (existingItem != null) { await sqlConnection.UpdateAsync(employmentCheckCacheRequest, tran); }
                        else { await sqlConnection.InsertAsync(employmentCheckCacheRequest, tran); }
                    }
                    catch (SqlException)
                    {
                        tran.Rollback();
                        throw;
                    }

                    await tran.CommitAsync();
                }
            }
        }

        public async Task<IList<EmploymentCheckCacheRequest>> SetReleatedRequestsRequestCompletionStatus(
            Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet
        )
        {
            var request = employmentCheckCacheRequestAndStatusToSet.Item1;
            var processingCompletionStatus = employmentCheckCacheRequestAndStatusToSet.Item2;

            // 'Related' requests are requests that have the same 'parent' EmploymentCheck
            // (i.e. the same ApprenticeEmploymentCheckId, which is the foreign key from the EmploymentCheck table)
            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));
                await sqlConnection.OpenAsync();

                // TODO: Dave to specify the criteria for the 'WHERE' clause to 'skip' the remaining requests
                var parameters = new DynamicParameters();
                parameters.Add("@Id", request.Id, DbType.Int64);
                parameters.Add("@ApprenticeEmploymentCheckId", request.ApprenticeEmploymentCheckId, DbType.Int64);
                parameters.Add("@nino", request.Nino, DbType.String);
                parameters.Add("@minDate", request.MinDate, DbType.DateTime);
                parameters.Add("@maxDate", request.MaxDate, DbType.DateTime);

                parameters.Add("@requestCompletionStatus", processingCompletionStatus, DbType.Int16);
                parameters.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                await sqlConnection.ExecuteAsync(
                    "UPDATE [Cache].[EmploymentCheckCacheRequest] " +
                    "SET    RequestCompletionStatus     =  @requestCompletionStatus, " +
                    "       LastUpdatedOn               =  @lastUpdatedOn " +
                    "WHERE  ApprenticeEmploymentCheckId =  @apprenticeEmploymentCheckId " +
                    "AND    Nino                        =  @nino " +
                    "AND    MinDate                     =  @minDate " +
                    "AND    MaxDate                     =  @maxDate " +
                    "AND    (Employed                   IS NULL OR Employed = 0) " +
                    "AND    RequestCompletionStatus     IS NULL ",
                    parameters,
                    commandType: CommandType.Text);

                var confirmUpdateParameters = new DynamicParameters();
                confirmUpdateParameters.Add("@Id", request.Id, DbType.Int64);
                confirmUpdateParameters.Add("@ApprenticeEmploymentCheckId", request.ApprenticeEmploymentCheckId, DbType.Int64);
                confirmUpdateParameters.Add("@CorrelationId", request.CorrelationId, DbType.Guid);
                confirmUpdateParameters.Add("@nino", request.Nino, DbType.String);
                confirmUpdateParameters.Add("@minDate", request.MinDate, DbType.DateTime);
                confirmUpdateParameters.Add("@maxDate", request.MaxDate, DbType.DateTime);

                var updatedRows = (await sqlConnection.QueryAsync<EmploymentCheckCacheRequest>(
                    sql: "SELECT * FROM [Cache].[EmploymentCheckCacheRequest] " +
                            "WHERE  Id                          <> @Id " +
                            "AND    ApprenticeEmploymentCheckId =  @apprenticeEmploymentCheckId " +
                            "ORDER BY Id ",
                    param: confirmUpdateParameters,
                    commandType: CommandType.Text)).AsList();

                return updatedRows;
            }
        }
    }
}