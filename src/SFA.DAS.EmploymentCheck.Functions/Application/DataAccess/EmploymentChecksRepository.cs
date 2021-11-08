//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Threading.Tasks;
//using Dapper;
//using Microsoft.Azure.Services.AppAuthentication;
//using Microsoft.Extensions.Logging;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
//using SFA.DAS.EmploymentCheck.Functions.Configuration;
//using SFA.DAS.EmploymentCheck.Functions.Helpers;

//namespace SFA.DAS.EmploymentCheck.Functions.Application.DataAccess
//{
//    public class EmploymentChecksRepository : IEmploymentChecksRepository
//    {
//        private const string AzureResource = "https://database.windows.net/";
//        private readonly string _connectionString;
//        private readonly int _batchSize;
//        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
//        private ILoggerAdapter<EmploymentChecksRepository> _logger;

//        public EmploymentChecksRepository(
//            ApplicationSettings applicationSettings,
//            AzureServiceTokenProvider azureServiceTokenProvider,
//            ILoggerAdapter<EmploymentChecksRepository> logger)
//        {
//            _connectionString = applicationSettings.DbConnectionString;
//            _azureServiceTokenProvider = azureServiceTokenProvider;
//            _batchSize = applicationSettings.BatchSize;
//            _logger = logger;
//        }

//        public async Task<List<Apprentice>> GetApprentices(SqlConnection sqlConnection)
//        {
//            var thisMethodName = "EmploymentChecksRepository.GetLearnersRequiringEmploymentChecks()";

//            List<Apprentice> learnerRequiringEmploymentCheckDto = null;
//            try
//            {
//                if (sqlConnection != null)
//                {
//                    await using (sqlConnection)
//                    {

//                        var parameters = new DynamicParameters();
//                        parameters.Add("@batchSize", _batchSize);

//                        await sqlConnection.OpenAsync();
//                        var result = await sqlConnection.QueryAsync<EmploymentCheckResult>(
//                            sql: "SELECT TOP (@batchSize) * FROM [dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate",
//                            param: parameters,
//                            commandType: CommandType.Text);

//                        if (result != null && result.Any())
//                        {
//                            Log.WriteLog(_logger, thisMethodName, $"Database query returned {learnerRequiringEmploymentCheckDto.Count} learners.");
//                            learnerRequiringEmploymentCheckDto = result.Select(x => new Apprentice(
//                                x.Id,
//                                x.AccountId,
//                                x.NationalInsuranceNumber,
//                                x.ULN,
//                                x.UKPRN,
//                                x.ApprenticeshipId,
//                                x.MinDate,
//                                x.MaxDate)).ToList();
//                        }
//                        else
//                        {
//                            Log.WriteLog(_logger, thisMethodName, $"Database query returned [0] learners.");
//                            learnerRequiringEmploymentCheckDto = new List<Apprentice>(); // return an empty list rather than null
//                        }
//                    }
//                }
//                else
//                {
//                    Log.WriteLog(_logger, thisMethodName, "*** ERROR ****: sqlConnection string is NULL, no learners requiring employment check were retrieved.");
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
//            }

//            return learnerRequiringEmploymentCheckDto;
//        }

//        public async Task<int> SaveEmploymentCheckResult(long id, bool result)
//        {
//            await using (var connection = await CreateConnection())
//            {
//                var parameters = new DynamicParameters();
//                parameters.Add("@id", id);
//                parameters.Add("@isEmployed", result);

//                await connection.OpenAsync();
//                await connection.ExecuteAsync(
//                    sql: "UPDATE dbo.EmploymentChecks SET IsEmployed = @isEmployed, LastUpdated = GETDATE(), HasBeenChecked = 1 WHERE Id = @id",
//                    commandType: CommandType.Text,
//                    param: parameters);
//            }

//            return await Task.FromResult(0);
//        }

//        public Task<int> SaveEmploymentCheckResult(long id, long uln, bool result)
//        {
//            //Required for stub
//            //TO DO Remove when stub is removed
//            throw new NotImplementedException();
//        }

//        private async Task<SqlConnection> CreateConnection()
//        {
//            var connection = new SqlConnection(_connectionString);
//            if (_azureServiceTokenProvider != null)
//            {
//                connection.AccessToken = await _azureServiceTokenProvider.GetAccessTokenAsync(AzureResource);
//            }

//            return connection;
//        }
//    }
//}
