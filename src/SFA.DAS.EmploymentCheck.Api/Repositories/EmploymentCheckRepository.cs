using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Api.Application.Helpers;
using SFA.DAS.EmploymentCheck.Api.Configuration;
using Models = SFA.DAS.EmploymentCheck.Api.Application.Models;

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
        public async Task<Models.EmploymentCheck> GetEmploymentCheck(Guid correlationId)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);

            var lastCheck = (await sqlConnection.GetAllAsync<Application.Models.EmploymentCheck>())
                .FirstOrDefault(x => x.CorrelationId == correlationId);

            return lastCheck;
        }

        public async Task Insert(Models.EmploymentCheck employmentCheck)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);

            await sqlConnection.InsertAsync(employmentCheck);
        }
    }
}