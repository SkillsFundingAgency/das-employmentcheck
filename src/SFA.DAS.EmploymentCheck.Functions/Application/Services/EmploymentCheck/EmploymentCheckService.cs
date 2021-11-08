using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : IEmploymentCheckService
    {
        private EmploymentCheckDbConfiguration _employmentCheckDbConfiguration;
        private const string AzureResource = "https://database.windows.net/";
        private readonly string _connectionString;
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private ILogger<IEmploymentCheckService> _logger;

        public EmploymentCheckService(
            ApplicationSettings applicationSettings,                            // TODO: Replace this generic application setting
            EmploymentCheckDbConfiguration employmentCheckDbConfiguration,      // TODO: With this specific employment check database configuration
            AzureServiceTokenProvider azureServiceTokenProvider,
            ILogger<IEmploymentCheckService> logger)
        {
            _connectionString = applicationSettings.DbConnectionString;
            _employmentCheckDbConfiguration = employmentCheckDbConfiguration;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
            _logger = logger;
        }

        public async Task<IList<Apprentice>> GetApprentices()
        {
            var thisMethodName = $"EmploymentCheckService.GetApprentices()";

            IList<Apprentice> apprentices = null;
            try
            {
                var sqlConnection = new SqlConnection(_connectionString);   // TODO: this needs to work for both the Azure Sql db and a local Sql Server db
                if (sqlConnection != null)
                {
                    await using (sqlConnection)
                    {

                        var parameters = new DynamicParameters();
                        parameters.Add("@batchSize", _batchSize);

                        await sqlConnection.OpenAsync();
                        var employmentCheckResults = await sqlConnection.QueryAsync<EmploymentCheckResult>(
                            sql: "SELECT TOP (@batchSize) * FROM [dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate",
                            param: parameters,
                            commandType: CommandType.Text);

                        if (employmentCheckResults != null && employmentCheckResults.Any())
                        {
                            Log.WriteLog(_logger, thisMethodName, $"Database query returned {employmentCheckResults.Count()} apprentices.");
                            apprentices = employmentCheckResults.Select(x => new Apprentice(
                                x.Id,
                                x.AccountId,
                                x.NationalInsuranceNumber,
                                x.ULN,
                                x.UKPRN,
                                x.ApprenticeshipId,
                                x.MinDate,
                                x.MaxDate)).ToList();
                        }
                        else
                        {
                            Log.WriteLog(_logger, thisMethodName, $"Database query returned [0] apprentices.");
                            apprentices = new List<Apprentice>(); // return an empty list rather than null
                        }
                    }
                }
                else
                {
                    Log.WriteLog(_logger, thisMethodName, "*** ERROR ****: sqlConnection string is NULL, no apprentices requiring employment check were retrieved.");
                }

                return apprentices;


            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

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

        public Task<int> SaveEmploymentCheckResult(long id, long uln, bool result)
        {
            //Required for stub
            //TO DO Remove when stub is removed
            throw new NotImplementedException();
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