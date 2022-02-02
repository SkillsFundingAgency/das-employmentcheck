using Ardalis.GuardClauses;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class EmploymentCheckCacheResponseRepository
        : IEmploymentCheckCacheResponseRepository
    {
        private readonly ILogger<EmploymentCheckCacheResponseRepository> _logger;
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckCacheResponseRepository(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null,
            Logger<EmploymentCheckCacheResponseRepository> logger = null
        )
        {
            _logger = logger;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task Save(EmploymentCheckCacheResponse response)
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
                    var existingItem = await sqlConnection.GetAsync<EmploymentCheckCacheResponse>(response.Id, tran);
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

        public async Task Insert(EmploymentCheckCacheResponse response)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.InsertAsync(response);
        }
    }
}