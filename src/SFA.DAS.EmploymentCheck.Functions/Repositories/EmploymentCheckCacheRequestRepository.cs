﻿using Ardalis.GuardClauses;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Data;
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

        public async Task InsertOrUpdate(EmploymentCheckCacheRequest request)
        {
            Guard.Against.Null(request, nameof(request));
            var requestType = request.GetType();

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
                            // Check there's a LastUpdatedOn property on the object before setting the timestamp
                            if (requestType.GetProperty("LastUpdatedOn") != null) { request.LastUpdatedOn = DateTime.Now; }
                            await sqlConnection.UpdateAsync(request, tran);
                        }
                        else
                        {
                            // Check there's a CreatedOn property on the object before setting the timestamp
                            if (requestType.GetProperty("CreatedOn") != null) { request.CreatedOn = DateTime.Now; }
                            await sqlConnection.InsertAsync(request, tran);
                        }

                        await tran.CommitAsync();
                    }
                    catch
                    {
                        await tran.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task Save(EmploymentCheckCacheRequest request)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.InsertAsync(request);
        }

        public async Task UpdateReleatedRequestsRequestCompletionStatus(EmploymentCheckCacheRequest request)
        {
            // 'Related' requests are requests that have the same 'parent' EmploymentCheck
            // (i.e. the same ApprenticeEmploymentCheckId, which is the foreign key from the EmploymentCheck table)
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.OpenAsync();

            // TODO: Dave to specify the criteria for the 'WHERE' clause to 'skip' the remaining requests
            var parameters = new DynamicParameters();
            parameters.Add("@ApprenticeEmploymentCheckId", request.ApprenticeEmploymentCheckId, DbType.Int64);
            parameters.Add("@nino", request.Nino, DbType.String);
            parameters.Add("@minDate", request.MinDate, DbType.DateTime);
            parameters.Add("@maxDate", request.MaxDate, DbType.DateTime);

            parameters.Add("@requestCompletionStatus", ProcessingCompletionStatus.Abandoned, DbType.Int16);
            parameters.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

            await sqlConnection.ExecuteAsync(
                "UPDATE [Cache].[EmploymentCheckCacheRequest] " +
                "SET    RequestCompletionStatus     = @requestCompletionStatus, " +
                "       Employed                    = null, " +
                "       LastUpdatedOn               = @lastUpdatedOn " +
                "WHERE  ApprenticeEmploymentCheckId = @apprenticeEmploymentCheckId " +
                "AND    Nino                        = @nino " +
                "AND    MinDate                     = @minDate " +
                "AND    MaxDate                     = @maxDate " +
                "AND    (Employed                   IS NULL OR Employed = 0) " +
                "AND    RequestCompletionStatus     IS NULL ",
                parameters,
                commandType: CommandType.Text);
        }
    }
}