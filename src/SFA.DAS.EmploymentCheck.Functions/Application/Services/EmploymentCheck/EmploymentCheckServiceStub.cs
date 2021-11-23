using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc.Formatters;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
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

        public EmploymentCheckServiceStub(
            ILogger<IEmploymentCheckService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database
        /// </summary>
        /// <returns>Task<IList<ApprenticeEmploymentCheckModel>></returns>
        public async override Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Service(long employmentCheckLastGetId)
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
                      employmentCheckLastGetId,
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


        public static ApprenticeEmploymentCheckMessageModel[] StubApprenticeEmploymentCheckMessageData => new []
        {
            new ApprenticeEmploymentCheckMessageModel()
            {
                PayeScheme = "123/AB12345",
                NationalInsuranceNumber = "SC111111A",
                StartDateTime = new DateTime(2010, 01, 01),
                EndDateTime = new DateTime(2018, 01, 01)
            },
            new ApprenticeEmploymentCheckMessageModel()
            {
                PayeScheme = "840/MODES17",
                NationalInsuranceNumber = "SC111111A",
                StartDateTime = new DateTime(2010, 01, 01),
                EndDateTime = new DateTime(2018, 01, 01)
            },
            new ApprenticeEmploymentCheckMessageModel()
            {
                PayeScheme = "840/MODES17",
                NationalInsuranceNumber = "AA123456C",
                StartDateTime = new DateTime(2010, 01, 01),
                EndDateTime = new DateTime(2018, 01, 01)
            },
            new ApprenticeEmploymentCheckMessageModel()
            {
                PayeScheme = "111/AA00001",
                NationalInsuranceNumber = "AA123456C",
                StartDateTime = new DateTime(2010, 01, 01),
                EndDateTime = new DateTime(2018, 01, 01)
            },
            new ApprenticeEmploymentCheckMessageModel()
            {
                PayeScheme = "840/HZ00064",
                NationalInsuranceNumber = "AS960509A",
                StartDateTime = new DateTime(2010, 01, 01),
                EndDateTime = new DateTime(2018, 01, 01)
            },
            new ApprenticeEmploymentCheckMessageModel()
            {
                PayeScheme = "923/EZ00059",
                NationalInsuranceNumber = "PR555555A",
                StartDateTime = new DateTime(2010, 01, 01),
                EndDateTime = new DateTime(2018, 01, 01)
            },
        };

    }
}
