using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

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
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        public async override Task<IList<Models.Domain.EmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Service(long employmentCheckLastGetId)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticeEmploymentChecksBatch_Service()";

            IList<Models.Domain.EmploymentCheckModel> apprenticeEmploymentChecks = null;
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
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticeEmploymentChecks;
        }

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue
        /// </summary>
        /// <param name="apprenticeEmploymentData"></param>
        /// <returns>Task</returns>
        public async override Task EnqueueApprenticeEmploymentCheckMessages_Service(EmploymentCheckData apprenticeEmploymentData)
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
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API
        /// </summary>
        /// <returns>Task<ApprenticeEmploymentCheckMessageModel></returns>
        public async override Task<EmploymentCheckMessage> DequeueApprenticeEmploymentCheckMessage_Service()
        {
            var thisMethodName = $"{ThisClassName}.DequeueApprenticeEmploymentCheckMessage_Service()";

            EmploymentCheckMessage apprenticeEmploymentCheckMessageModel = null;
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
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return apprenticeEmploymentCheckMessageModel;
        }

        public async override Task SaveEmploymentCheckResult_Service(EmploymentCheckMessage apprenticeEmploymentCheckMessageModel)
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
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
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


        public static EmploymentCheckMessage[] StubApprenticeEmploymentCheckMessageData => new []
        {
            new EmploymentCheckMessage()
            {
                PayeScheme = "123/AB12345",
                NationalInsuranceNumber = "SC111111A",
                MinDateTime = new DateTime(2010, 01, 01),
                MaxDateTime = new DateTime(2018, 01, 01)
            },
            new EmploymentCheckMessage()
            {
                PayeScheme = "840/MODES17",
                NationalInsuranceNumber = "SC111111A",
                MinDateTime = new DateTime(2010, 01, 01),
                MaxDateTime = new DateTime(2018, 01, 01)
            },
            new EmploymentCheckMessage()
            {
                PayeScheme = "840/MODES17",
                NationalInsuranceNumber = "AA123456C",
                MinDateTime = new DateTime(2010, 01, 01),
                MaxDateTime = new DateTime(2018, 01, 01)
            },
            new EmploymentCheckMessage()
            {
                PayeScheme = "111/AA00001",
                NationalInsuranceNumber = "AA123456C",
                MinDateTime = new DateTime(2010, 01, 01),
                MaxDateTime = new DateTime(2018, 01, 01)
            },
            new EmploymentCheckMessage()
            {
                PayeScheme = "840/HZ00064",
                NationalInsuranceNumber = "AS960509A",
                MinDateTime = new DateTime(2010, 01, 01),
                MaxDateTime = new DateTime(2018, 01, 01)
            },
            new EmploymentCheckMessage()
            {
                PayeScheme = "923/EZ00059",
                NationalInsuranceNumber = "PR555555A",
                MinDateTime = new DateTime(2010, 01, 01),
                MaxDateTime = new DateTime(2018, 01, 01)
            },
        };

    }
}
