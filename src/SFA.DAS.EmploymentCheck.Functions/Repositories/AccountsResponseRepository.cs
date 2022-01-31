using Ardalis.GuardClauses;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class AccountsResponseRepository
        : IAccountsResponseRepository
    {
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public AccountsResponseRepository(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null
        )
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task InsertOrUpdate(AccountsResponse response)
        {
            Guard.Against.Null(response, nameof(response));

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));
                var responseType = response.GetType();

                await sqlConnection.OpenAsync();
                using var tran = await sqlConnection.BeginTransactionAsync();
                try
                {
                    var existingItem = await sqlConnection.GetAsync<AccountsResponse>(response.Id, tran);
                    if (existingItem != null)
                    {
                        // Check there's a LastUpdatedOn property on the object before setting the timestamp
                        if (responseType.GetProperty("LastUpdatedOn") != null) { response.LastUpdatedOn = DateTime.Now; }
                        await sqlConnection.UpdateAsync(response, tran);
                    }
                    else
                    {
                        // Check there's a CreatedOn property on the object before setting the timestamp
                        if (responseType.GetProperty("CreatedOn") != null) { response.CreatedOn = DateTime.Now; }
                        await sqlConnection.InsertAsync(response, tran);
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

        public async Task Save(AccountsResponse response)
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