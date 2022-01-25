using Ardalis.GuardClauses;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public class DataCollectionsResponseRepository : IDataCollectionsResponseRepository
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public DataCollectionsResponseRepository(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null)
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _applicationSettings = applicationSettings;
        }

        public async Task Save(DataCollectionsResponse dataCollectionsResponse)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _applicationSettings,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));


            await sqlConnection.InsertAsync(dataCollectionsResponse);
        }

        public async Task<DataCollectionsResponse> Get(DataCollectionsResponse dataCollectionsResponse)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _applicationSettings,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            var result = await sqlConnection.GetAsync<DataCollectionsResponse>(dataCollectionsResponse
                    .ApprenticeEmploymentCheckId);

            return result;
        }
    }
}