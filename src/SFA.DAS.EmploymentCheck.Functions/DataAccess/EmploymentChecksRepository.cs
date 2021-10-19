using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Models;

namespace SFA.DAS.EmploymentCheck.Functions.DataAccess
{
    public class EmploymentChecksRepository : IEmploymentChecksRepository
    {
        private const string AzureResource = "https://database.windows.net/";
        private readonly string _connectionString;
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private ILogger<EmploymentChecksRepository> _logger;

        public EmploymentChecksRepository(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider,
            ILogger<EmploymentChecksRepository> logger)
        {
            _connectionString = applicationSettings.DbConnectionString;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
            _logger = logger;
        }

        public async Task<List<ApprenticeToVerifyDto>> GetApprenticesToCheck()
        {
            var thisMethodName = "EmploymentChecksRepository.EmploymentChecksRepository.GetApprenticesToCheck()";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            //_logger.LogInformation($"{messagePrefix} Started.");

            List<ApprenticeToVerifyDto> apprentices = null;

            try
            {
                //_logger.LogInformation($"{messagePrefix} Executing database query [SELECT TOP({_batchSize}) * FROM[dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate].");
                await using (var connection = await CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@batchSize", _batchSize);

                    await connection.OpenAsync();
                    var result = await connection.QueryAsync<EmploymentCheckResult>(
                        sql: "SELECT TOP (@batchSize) * FROM [dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate",
                        param: parameters,
                        commandType: CommandType.Text);

                    if(result != null && result.Any() )
                    {
                        _logger.LogInformation($"{messagePrefix} Database query returned {apprentices.Count} apprentices.");
                        apprentices = result.Select(x => new ApprenticeToVerifyDto(x.Id, x.AccountId, x.NationalInsuranceNumber, x.ULN, x.UKPRN, x.ApprenticeshipId, x.MinDate, x.MaxDate)).ToList();
                    }
                    else
                    {
                        _logger.LogInformation($"{messagePrefix} Database query returned null/zero apprentices.");
                        apprentices = new List<ApprenticeToVerifyDto>(); // return an empty list rather than null
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            //_logger.LogInformation($"{messagePrefix} Completed.");
            return apprentices;
        }

        public async Task<int> SaveEmploymentCheckResult(long id, bool result)
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

            return await Task.FromResult(0);
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
