﻿using Ardalis.GuardClauses;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public class DataCollectionsResponseRepository
        : IDataCollectionsResponseRepository
    {
        private readonly ILogger<DataCollectionsResponseRepository> _logger;
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public DataCollectionsResponseRepository(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null,
            Logger<DataCollectionsResponseRepository> logger = null
        )
        {
            _logger = logger;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task InsertOrUpdate(DataCollectionsResponse response)
        {
            Guard.Against.Null(response, nameof(response));

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));

                await sqlConnection.OpenAsync();
                using var tran = await sqlConnection.BeginTransactionAsync();
                try
                {
                    var existingItem = await sqlConnection.GetAsync<DataCollectionsResponse>(response.Id, tran);
                    if (existingItem != null)
                    {
                        response.LastUpdatedOn = DateTime.Now;
                        await sqlConnection.UpdateAsync(response, tran);
                    }
                    else
                    {
                        try
                        {
                            response.LastUpdatedOn = null;
                            response.CreatedOn = DateTime.Now;
                            await sqlConnection.InsertAsync(response, tran);
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

        public async Task Save(DataCollectionsResponse response)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.InsertAsync(response);
        }

        public async Task<DataCollectionsResponse> GetByEmploymentCheckId(long apprenticeEmploymentCheckId)
        {
            DataCollectionsResponse response = null;
            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();

                var parameter = new DynamicParameters();
                parameter.Add("@ApprenticeEmploymentCheckId", apprenticeEmploymentCheckId, DbType.Int64);

                response = (await sqlConnection.QueryAsync<DataCollectionsResponse>(
                    sql: "SELECT    TOP(1) * " +
                         "FROM      [Cache].[DataCollectionsResponse] " +
                         "WHERE     ApprenticeEmploymentCheckId = @ApprenticeEmploymentCheckId " +
                         "ORDER BY  CreatedOn DESC ",
                    parameter,
                    commandType: CommandType.Text)).FirstOrDefault();
            }

            return response;
        }
    }
}