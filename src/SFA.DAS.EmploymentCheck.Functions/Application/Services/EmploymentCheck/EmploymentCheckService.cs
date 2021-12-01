using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : EmploymentCheckServiceBase
    {
        private const string ThisClassName = "\n\nEmploymentCheckService";
        private const string AzureResource = "https://database.windows.net/";
        private readonly string _connectionString;
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private ILogger<IEmploymentCheckService> _logger;

        /// <summary>
        /// The production implementation of the methods definined in the EmploymentCheckService
        /// </summary>
        /// <param name="applicationSettings"></param>
        /// <param name="employmentCheckDbConfiguration"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="logger"></param>
        public EmploymentCheckService(
            ApplicationSettings applicationSettings,                            // TODO: Replace this generic application setting
            AzureServiceTokenProvider azureServiceTokenProvider,
            ILogger<IEmploymentCheckService> logger)
        {
            _connectionString = applicationSettings.DbConnectionString;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
            _logger = logger;
        }

        /// <summary>
        /// Gets a batch of the employment checks from the Employment Check database
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
                      _batchSize,
                      employmentCheckLastHighestBatchId,
                      _azureServiceTokenProvider);
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
            // TODO: Add implementation for using Azure SqlDatabase
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

                if(employmentCheckMessage == null)
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

        public async override Task SaveEmploymentCheckResult_Service(
            EmploymentCheckMessage employmentCheckMessage)
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

        public async override Task SeedEmploymentCheckDatabaseTableTestData()
        {
            var thisMethodName = $"{ThisClassName}.SeedEmploymentCheckDatabaseTableTestData()";

            try
            {
                await SeedEmploymentCheckDatabaseTableTestData(
                    _logger,
                    _connectionString,
                    AzureResource,
                    _azureServiceTokenProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}