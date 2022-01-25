using Ardalis.GuardClauses;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class EmploymentCheckRepository
        : IEmploymentCheckRepository
    {
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckRepository(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null
        )
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task Save(Models.EmploymentCheck check)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));


            await sqlConnection.InsertAsync(check);
        }

        public async Task InsertOrUpdate(Models.EmploymentCheck check)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection
            (
                _connectionString,
                _azureServiceTokenProvider
            );

            await sqlConnection.OpenAsync();

            await using var transaction = sqlConnection.BeginTransaction();
            var existingItem = await sqlConnection.GetAsync<Models.EmploymentCheck>(check.Id, transaction);

            if (existingItem != null) await sqlConnection.UpdateAsync(check, transaction);
            else await sqlConnection.InsertAsync(check, transaction);

            transaction.Commit();
        }
    }
}