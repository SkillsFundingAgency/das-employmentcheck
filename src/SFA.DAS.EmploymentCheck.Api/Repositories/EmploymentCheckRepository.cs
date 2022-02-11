using Ardalis.GuardClauses;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Api.Configuration;

namespace SFA.DAS.EmploymentCheck.Api.Repositories
{
    public class EmploymentCheckRepository : IEmploymentCheckRepository
    {
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckRepository(
            EmploymentCheckSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null)
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }
        public async Task<Application.Models.EmploymentCheck> GetEmploymentCheck(Guid correlationId)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            var lastCheck = (await sqlConnection.GetAllAsync<Application.Models.EmploymentCheck>())
                .FirstOrDefault(x => x.CorrelationId == correlationId);

            return lastCheck;
        }

        public async Task Insert(Application.Models.EmploymentCheck employmentCheck)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.InsertAsync(employmentCheck);
        }
    }
}