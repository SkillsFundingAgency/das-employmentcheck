using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Models;

namespace SFA.DAS.EmploymentCheck.Functions.DataAccess
{
    public class EmploymentChecksRepository : IEmploymentChecksRepository
    {
        private const string AzureResource = "https://database.windows.net/";
        private readonly string _connectionString;
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentChecksRepository(ApplicationSettings applicationSettings, AzureServiceTokenProvider azureServiceTokenProvider)
        {
            _connectionString = applicationSettings.DbConnectionString;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
        }

        public async Task<List<ApprenticeToVerifyDto>> GetApprenticesToCheck()
        {
            await using (var connection = await CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@batchSize", _batchSize);

                await connection.OpenAsync();
                var result = await connection.QueryAsync<EmploymentCheckResult>(
                    sql: "SELECT TOP (@batchSize) * FROM [dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate",
                    param: parameters,
                    commandType: CommandType.Text);

                return result.Select(x => new ApprenticeToVerifyDto(x.Id, x.AccountId, x.NationalInsuranceNumber, x.ULN, x.UKPRN, x.ApprenticeshipId, x.MinDate, x.MaxDate)).ToList();
            }
        }

        public async Task SaveEmploymentCheckResult(long id, bool result)
        {
            await using (var connection = await CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", id);
                parameters.Add("@isEmployed", result);

                await connection.OpenAsync();
                await connection.ExecuteAsync(
                    sql: "UPDATE dbo.EmploymentChecks SET IsEmployed = @isEmployed, LastUpdated = GETDATE(), HasBeenChecked = 1 WHERE Id = @id",
                    commandType: CommandType.Text,
                    param: parameters);
            }
        }

        private async Task<SqlConnection> CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            if (_azureServiceTokenProvider != null)
            {
                connection.AccessToken = await _azureServiceTokenProvider.GetAccessTokenAsync(AzureResource);
            }

            return connection;
        }
    }
}
