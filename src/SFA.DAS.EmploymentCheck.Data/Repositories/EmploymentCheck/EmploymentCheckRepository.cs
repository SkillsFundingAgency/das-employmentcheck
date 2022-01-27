using System;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public class EmploymentCheckRepository : IEmploymentCheckRepository
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckRepository(
            ApplicationSettings applicationApplicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null)
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _applicationSettings = applicationApplicationSettings;
        }

        public async Task Save(Models.EmploymentCheck employmentCheck)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _applicationSettings,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));


            await sqlConnection.InsertAsync(employmentCheck);
        }

        public async Task<Models.EmploymentCheck> GetEmploymentCheck(Guid correlationId)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _applicationSettings,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            var lastCheck = (await sqlConnection.GetAllAsync<Models.EmploymentCheck>())
                .FirstOrDefault(x => x.CorrelationId == correlationId);

            return lastCheck;
        }

        public async Task Insert(Models.EmploymentCheck employmentCheck)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _applicationSettings,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.InsertAsync(employmentCheck);
        }
    }
}
