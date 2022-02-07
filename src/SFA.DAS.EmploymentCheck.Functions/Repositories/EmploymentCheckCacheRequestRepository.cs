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
using System.Linq;
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
                    try
                    {
                        var existingItem = await sqlConnection.GetAsync<EmploymentCheckCacheRequest>(employmentCheckCacheRequest.Id, tran);
                        if (existingItem != null) { await sqlConnection.UpdateAsync(employmentCheckCacheRequest, tran); }
                        else { await sqlConnection.InsertAsync(employmentCheckCacheRequest, tran); }
                    }
                    catch (SqlException)
                    {
                        await tran.RollbackAsync();
                        throw;
                    }

                    await tran.CommitAsync();
                }
            }
        }

        public async Task<IList<EmploymentCheckCacheRequest>> SetRelatedRequestsCompletionStatus(
            Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet
        )
        {
            var request = employmentCheckCacheRequestAndStatusToSet.Item1;
            var processingCompletionStatus = employmentCheckCacheRequestAndStatusToSet.Item2;

            // 'Related' requests are requests that have the same 'parent' EmploymentCheck
            // (i.e. the same ApprenticeEmploymentCheckId, which is the foreign key from the EmploymentCheck table)
            IList<EmploymentCheckCacheRequest> employmentCheckCacheRequests = null;
            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();
                using (var transaction = await sqlConnection.BeginTransactionAsync())
                {
                    try
                    {
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

                        employmentCheckCacheRequests = (await sqlConnection.QueryAsync<EmploymentCheckCacheRequest>(
                            sql: "SELECT * FROM [Cache].[EmploymentCheckCacheRequest] " +
                                    "WHERE  Id                          <> @Id " +
                                    "AND    ApprenticeEmploymentCheckId =  @apprenticeEmploymentCheckId " +
                                    "ORDER BY Id ",
                            param: confirmUpdateParameters,
                            commandType: CommandType.Text)).AsList();

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }

                    return employmentCheckCacheRequests;
                }
            }
        }

       public async Task UpdateEmployedAndRequestStatusFields(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();

                var parameter = new DynamicParameters();
                parameter.Add("@Id", employmentCheckCacheRequest.Id, DbType.Int64);
                parameter.Add("@employed", employmentCheckCacheRequest.Employed, DbType.Boolean);
                parameter.Add("@requestCompletionStatus", employmentCheckCacheRequest.RequestCompletionStatus, DbType.Int16);
                parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                await sqlConnection.ExecuteAsync(
                    "UPDATE [Cache].[EmploymentCheckCacheRequest] " +
                    "SET Employed = @employed, requestCompletionStatus = @requestCompletionStatus, LastUpdatedOn = @lastUpdatedOn " +
                    "WHERE Id = @Id ",
                    parameter,
                    commandType: CommandType.Text);
            }
        }

        public async Task<EmploymentCheckCacheRequest> GetNext()
        {
            EmploymentCheckCacheRequest employmentCheckCacheRequest = null;

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();
                using (var transaction = await sqlConnection.BeginTransactionAsync())
                {
                    try
                    {
                        employmentCheckCacheRequest = (await sqlConnection.QueryAsync<EmploymentCheckCacheRequest>(
                            sql: "SELECT    TOP(1) * " +
                                 "FROM      [Cache].[EmploymentCheckCacheRequest] " +
                                 "WHERE     (RequestCompletionStatus IS NULL)" +
                                 "ORDER BY  Id",
                            commandType: CommandType.Text,
                            transaction: transaction)).FirstOrDefault();

                        // Set the RequestCompletionStatus to 'Started' so that this request doesn't get read the next time around
                        if (employmentCheckCacheRequest != null)
                        {
                            var parameter = new DynamicParameters();
                            parameter.Add("@Id", employmentCheckCacheRequest.Id, DbType.Int64);
                            parameter.Add("@requestCompletionStatus", ProcessingCompletionStatus.Started, DbType.Int16);
                            parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                            await sqlConnection.ExecuteAsync(
                                "UPDATE [Cache].[EmploymentCheckCacheRequest] " +
                                "SET    RequestCompletionStatus = @requestCompletionStatus, " +
                                "       LastUpdatedOn = @lastUpdatedOn " +
                                "WHERE  Id = @Id ",
                                parameter,
                                commandType: CommandType.Text,
                                transaction: transaction);
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                return employmentCheckCacheRequest;
            }
        }
    }
}