using System;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Api.Repositories
{
    public class EmploymentCheckRepository : IEmploymentCheckRepository
    {
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckRepository(
            ApplicationSettings applicationSettings,
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

            var lastCheck = sqlConnection.GetAllAsync<Application.Models.EmploymentCheck>().Result.Select(x => x)
                .Where(x => x.CorrelationId == correlationId).OrderBy(x => x.CorrelationId);
            
            if (lastCheck.Count() != 0)
            {
                return lastCheck.Select(x => x).Last();
            }
            return new Application.Models.EmploymentCheck();
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

        public async Task<int> GetLastId()
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            var lastCheck = sqlConnection.GetAllAsync<Application.Models.EmploymentCheck>().Result.Select(x => x).Last();

            return (int)lastCheck.Id;
        }
    }
}