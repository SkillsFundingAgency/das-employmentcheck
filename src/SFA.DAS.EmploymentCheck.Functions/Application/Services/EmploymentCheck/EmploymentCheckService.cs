﻿using Microsoft.Azure.Services.AppAuthentication;
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
                      _batchSize,
                      employmentCheckLastGetId,
                      _azureServiceTokenProvider);
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
        /// <param name="employmentCheckData"></param>
        /// <returns>Task</returns>
        public async override Task EnqueueApprenticeEmploymentCheckMessages_Service(EmploymentCheckData employmentCheckData)
        {
            // TODO: Add implementation for using Azure SqlDatabase
            var thisMethodName = $"{ThisClassName}.EnqueueApprenticeEmploymentCheckMessages_Service()";

            try
            {
                if (employmentCheckData != null)
                {
                    await EnqueueApprenticeEmploymentCheckMessages_Service(
                        _logger,
                        _connectionString,
                        AzureResource,
                        _azureServiceTokenProvider,
                        employmentCheckData);
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

                if(apprenticeEmploymentCheckMessageModel == null)
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
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModel input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        public async override Task SeedEmploymentCheckApprenticeDatabaseTableTestData()
        {
            var thisMethodName = $"{ThisClassName}.SeedEmploymentCheckApprenticeDatabaseTableTestData()";

            try
            {
                await SeedEmploymentCheckApprenticeDatabaseTableTestData(
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