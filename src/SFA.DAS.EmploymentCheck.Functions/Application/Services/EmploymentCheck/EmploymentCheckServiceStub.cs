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
        public async override Task<IList<Models.Domain.EmploymentCheckModel>> GetEmploymentChecksBatch_Service(long employmentCheckLastHighestBatchId)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentChecksBatch_Service()";

            IList<Models.Domain.EmploymentCheckModel> employmentCheckModels = null;
            try
            {
                employmentCheckModels = await GetEmploymentChecks_Base(
                      _logger,
                      _connectionString,
                      AzureResource,
                      10, // _batchSize // TODO: Setup in config
                      employmentCheckLastHighestBatchId,
                      _azureServiceTokenProvider);

                if(employmentCheckModels == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckModels return parameter from the base class service call to GetEmploymentChecks_Base returned null.");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckModels;
        }

        /// <summary>
        /// Adds an employment check message to the HMRC API message queue
        /// </summary>
        /// <param name="employmentCheckData"></param>
        /// <returns>Task</returns>
        public async override Task EnqueueEmploymentCheckMessages_Service(EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{ThisClassName}.EnqueueEmploymentCheckMessages_Service()";

            try
            {
                if (employmentCheckData != null)
                {
                    await EnqueueEmploymentCheckMessages_Service(
                        _logger,
                        _connectionString,
                        AzureResource,
                        _azureServiceTokenProvider,
                        employmentCheckData);

                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckData input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets an employment check message from the HMRC API message queue
        /// </summary>
        /// <returns>Task<EmploymentCheckMessage></returns>
        public async override Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage_Service()
        {
            var thisMethodName = $"{ThisClassName}.DequeueEmploymentCheckMessage_Service()";

            EmploymentCheckMessage employmentCheckMessage = null;
            try
            {
                employmentCheckMessage = await DequeueEmploymentCheckMessage_Base(
                    _logger,
                    _connectionString,
                    _batchSize,
                    AzureResource,
                    _azureServiceTokenProvider);

                if (employmentCheckMessage == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessage value returned from the call to DequeueEmploymentCheckMessage_Base() is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentCheckMessage;
        }

        public async override Task SaveEmploymentCheckResult_Service(EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckResult_Service()";

            try
            {
                if (employmentCheckMessage != null)
                {
                    await SaveEmploymentCheckResult_Base(
                         _logger,
                         _connectionString,
                         AzureResource,
                         _azureServiceTokenProvider,
                         employmentCheckMessage);
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessage input parameter is null.");
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
        public async override Task SeedEmploymentCheckDatabaseTableTestData()
        {
            await SeedEmploymentCheckDatabaseTableTestData(
                _logger,
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider);
        }


        public static EmploymentCheckMessage[] StubEmploymentCheckMessageData => new []
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
