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
    public class EmploymentCheckCacheRequestRepository : IEmploymentCheckCacheRequestRepository
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

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));

                if(request.CreatedOn == DateTime.MinValue)
                {
                    request.CreatedOn = DateTime.Now;
                }

                if (request.LastUpdatedOn == DateTime.MinValue)
                {
                    request.LastUpdatedOn = DateTime.Now;
                }

                await sqlConnection.OpenAsync();
                using var tran = await sqlConnection.BeginTransactionAsync();
                try
                {
                    var existingItem = await sqlConnection.GetAsync<EmploymentCheckCacheRequest>(request.Id, tran);
                    if (existingItem != null)
                    {
                        await sqlConnection.UpdateAsync(request, tran);
                    }
                    else
                    {
                        await sqlConnection.InsertAsync(request, tran);
                    }

                    await tran.CommitAsync();
                }
                catch(Exception ex)
                {
                    await tran.RollbackAsync();
                    throw;
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

        public async Task UpdateRelatedRequests(EmploymentCheckCacheRequest request)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.OpenAsync();

            // TODO: Dave to specify the criteria for 'which' requests to skip after an positive employment check
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