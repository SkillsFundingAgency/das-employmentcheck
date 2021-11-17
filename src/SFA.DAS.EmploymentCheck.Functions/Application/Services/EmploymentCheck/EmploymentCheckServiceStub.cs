using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Services;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    /// <summary>
    /// A stub implementation of the EmploymentCheckService for local development
    /// Note: The use of this stub or the real service at run-time is determined
    /// by an #if DEBUG conditional check in the ServiceCollectionExtension.cs class
    /// </summary>
    public class EmploymentCheckServiceStub
        : EmploymentCheckServiceBase
    {
        private const string ThisClassName = "\n\nEmploymentCheckServiceStub";

        private EmploymentCheckDbConfiguration _employmentCheckDbConfiguration;
        private const string AzureResource = "https://database.windows.net/";
        private readonly string _connectionString = System.Environment.GetEnvironmentVariable($"EmploymentChecksConnectionString"); // TODO: move to config
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private ILogger<IEmploymentCheckService> _logger;
        private IRandomNumberService _randomNumberService;

        public EmploymentCheckServiceStub(
            IRandomNumberService randomNumberService,
            ILogger<IEmploymentCheckService> logger)
        {
            _randomNumberService = randomNumberService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database
        /// </summary>
        /// <returns>Task<IList<ApprenticeEmploymentCheckModel>></returns>
        public async override Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Service()
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticeEmploymentChecksBatch_Service()";

            IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentChecks = null;
            try
            {
                apprenticeEmploymentChecks = await GetApprenticeEmploymentChecks_Base(
                      _logger,
                      _connectionString,
                      AzureResource,
                      10, // _batchSize // TODO: Setup in config
                      _azureServiceTokenProvider);

                if(apprenticeEmploymentChecks == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentChecks return parameter from the base class service call to GetApprenticeEmploymentCheck returned null.");

                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticeEmploymentChecks;
        }

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue
        /// </summary>
        /// <param name="apprenticeEmploymentData"></param>
        /// <returns>Task</returns>
        public async override Task EnqueueApprenticeEmploymentCheckMessages_Service(ApprenticeRelatedData apprenticeEmploymentData)
        {
            var thisMethodName = $"{ThisClassName}.EnqueueApprenticeEmploymentCheckMessages_Service()";

            try
            {
                if (apprenticeEmploymentData != null)
                {
                    await EnqueueApprenticeEmploymentCheckMessages_Service(
                        _logger,
                        _connectionString,
                        AzureResource,
                        _azureServiceTokenProvider,
                        apprenticeEmploymentData);

                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentChecks return value from the base class service call to GetApprenticeEmploymentCheck returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API
        /// </summary>
        /// <returns>Task<ApprenticeEmploymentCheckMessageModel></returns>
        public async override Task<ApprenticeEmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Service()
        {
            var thisMethodName = $"{ThisClassName}.DequeueApprenticeEmploymentCheckMessage_Service()";

            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel = null;
            try
            {
                apprenticeEmploymentCheckMessageModel = await DequeueApprenticeEmploymentCheckMessage_Base(
                    _logger,
                    _connectionString,
                    _batchSize,
                    AzureResource,
                    _azureServiceTokenProvider);

                if (apprenticeEmploymentCheckMessageModel == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModel value returned from the call to DequeueApprenticeEmploymentCheckMessage_Base() is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return apprenticeEmploymentCheckMessageModel;
        }

        public async override Task SaveEmploymentCheckResult_Service(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckResult_Service()";

            try
            {
                if (apprenticeEmploymentCheckMessageModel != null)
                {
                    await SaveEmploymentCheckResult_Base(
                         _logger,
                         _connectionString,
                         AzureResource,
                         _azureServiceTokenProvider,
                         apprenticeEmploymentCheckMessageModel);
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModel input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        //public async override Task<int> SaveEmploymentCheckResult(long id, long uln, bool result)
        //{
        //    var thisMethodName = "EmploymentCheckServiceStub.SaveEmploymentCheckResult()";

        //    Log.WriteLog(_logger, thisMethodName, $"Starting employment check save for ULN: [{uln}].");

        //    var parameters = new DynamicParameters();

        //    parameters.Add("id", id, DbType.Int64);
        //    parameters.Add("result", result, DbType.Boolean);
        //    parameters.Add("checked", true);

        //    var connection = new SqlConnection(_connectionString);

        //    await connection.OpenAsync();

        //    Log.WriteLog(_logger, thisMethodName, $"Updating row for ULN: {uln}.");
        //    return await connection.ExecuteAsync(
        //        "UPDATE [dbo].[EmploymentChecks] SET IsEmployed = @result, LastUpdated = GETDATE(), HasBeenChecked = @checked WHERE Id = @id",
        //            parameters,
        //            commandType: CommandType.Text);
        //}


        /// <summary>
        /// ----------------------------------
        /// TESTING ONLY - Generates test data
        /// ----------------------------------
        /// </summary>
        /// <returns></returns>
        public async override Task SeedEmploymentCheckApprenticeDatabaseTableTestData()
        {
            await SeedEmploymentCheckApprenticeDatabaseTableTestData(
                _logger,
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider);
        }

        // ------------------------------------------------------------------------
        // The code below can be deleted once we're sure that it's no longer needed
        // ------------------------------------------------------------------------

        //public async Task<List<Apprentice>> GetApprentices(SqlConnection sqlConnection)
        //{
        //    var thisMethodName = "EmploymentCheckServiceStub.GetLearnersRequiringEmploymentChecks()";
        //    List<Apprentice> learnersRequiringEmploymentCheckDto = null;

        //    await sqlConnection.OpenAsync();

        //    var result = await sqlConnection.QueryAsync<EmploymentCheckResult>(
        //        "SELECT TOP 5 * FROM [dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate",
        //        commandType: CommandType.Text);

        //    if (result != null && result.Any())
        //    {
        //        learnersRequiringEmploymentCheckDto = result.Select(x => new Apprentice(
        //            x.Id,
        //            x.AccountId,
        //            x.NationalInsuranceNumber,
        //            x.ULN,
        //            x.UKPRN,
        //            x.ApprenticeshipId,
        //            x.MinDate,
        //            x.MaxDate)).ToList();
        //        Log.WriteLog(_logger, thisMethodName, $"Database query returned {learnersRequiringEmploymentCheckDto.Count} learners.");
        //    }
        //    else
        //    {
        //        Log.WriteLog(_logger, thisMethodName, $"Database query returned [0] learners.");
        //        learnersRequiringEmploymentCheckDto = new List<Apprentice>(); // return an empty list rather than null
        //    }

        //    return learnersRequiringEmploymentCheckDto;
        //}

        /// <summary>
        /// Get a list of apprentices requiring an employment check
        /// </summary>
        /// <returns>List<Apprentice></returns>
        //public async override Task<IList<Apprentice>> GetApprenticeEmploymentChecks()
        //{
        //    //await SeedEmploymentCheckApprenticeDatabaseTableTestData(
        //    //   _logger,
        //    //    _connectionString,
        //    //    AzureResource,
        //    //    _azureServiceTokenProvider);

        //    var apprentices = new List<Apprentice>
        //    {
        //        new Apprentice
        //        {
        //            Id = 1,
        //            AccountId = 1,
        //            ULN = 1000000001,
        //            UKPRN = 10000001,
        //            ApprenticeshipId = 1,
        //            StartDate = new DateTime(2021, 11, 1),
        //            EndDate = new DateTime(2021, 11, 2)
        //        },
        //        new Apprentice
        //        {
        //            Id = 2,
        //            AccountId = 2,
        //            ULN = 2000000002,
        //            UKPRN = 20000002,
        //            ApprenticeshipId = 2,
        //            StartDate = new DateTime(2021, 11, 1),
        //            EndDate = new DateTime(2021, 11, 2)
        //        },
        //        new Apprentice
        //        {
        //            Id = 3,
        //            AccountId = 3,
        //            ULN = 3000000003,
        //            UKPRN = 30000003,
        //            ApprenticeshipId = 3,
        //            StartDate = new DateTime(2021, 11, 1),
        //            EndDate = new DateTime(2021, 11, 2)
        //        },
        //        new Apprentice
        //        {
        //            Id = 4,
        //            AccountId = 4,
        //            ULN = 4000000004,
        //            UKPRN = 40000004,
        //            ApprenticeshipId = 4,
        //            StartDate = new DateTime(2021, 11, 1),
        //            EndDate = new DateTime(2021, 11, 2)
        //        },
        //        new Apprentice
        //        {
        //            Id = 5,
        //            AccountId = 5,
        //            ULN = 5000000005,
        //            UKPRN = 50000005,
        //            ApprenticeshipId = 5,
        //            StartDate = new DateTime(2021, 11, 1),
        //            EndDate = new DateTime(2021, 11, 2)
        //        },
        //        new Apprentice
        //        {
        //            Id = 6,
        //            AccountId = 6,
        //            ULN = 6000000006,
        //            UKPRN = 60000006,
        //            ApprenticeshipId = 6,
        //            StartDate = new DateTime(2021, 11, 1),
        //            EndDate = new DateTime(2021, 11, 2)
        //        },
        //        new Apprentice
        //        {
        //            Id = 7,
        //            AccountId = 7,
        //            ULN = 7000000007,
        //            UKPRN = 70000007,
        //            ApprenticeshipId = 7,
        //            StartDate = new DateTime(2021, 11, 1),
        //            EndDate = new DateTime(2021, 11, 2)
        //        },
        //        new Apprentice
        //        {
        //            Id = 8,
        //            AccountId = 8,
        //            ULN = 8000000008,
        //            UKPRN = 80000008,
        //            ApprenticeshipId = 8,
        //            StartDate = new DateTime(2021, 11, 1),
        //            EndDate = new DateTime(2021, 11, 2)
        //        },
        //        new Apprentice
        //        {
        //            Id = 9,
        //            AccountId = 9,
        //            ULN = 9000000009,
        //            UKPRN = 90000009,
        //            ApprenticeshipId = 9,
        //            StartDate = new DateTime(2021, 11, 1),
        //            EndDate = new DateTime(2021, 11, 2)
        //        },
        //    };

        //    return await Task.FromResult(apprentices);
        //}
    }
}
