using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using SFA.DAS.EmploymentCheck.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : IEmploymentCheckService
    {
        private readonly ILogger<IEmploymentCheckService> _logger;
        private readonly ApplicationSettings _applicationSettings;
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly IEmploymentCheckRepository _employmentCheckRepository;
        private readonly IEmploymentCheckCacheRequestRepository _employmentCheckCacheRequestRepository;

        public EmploymentCheckService(
            ILogger<IEmploymentCheckService> logger,
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider,
            IEmploymentCheckRepository employmentCheckRepository,
            IEmploymentCheckCacheRequestRepository employmentCheckCacheRequestRepository
        )
        {
            _logger = logger;
            _applicationSettings = applicationSettings;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
            _employmentCheckRepository = employmentCheckRepository;
            _employmentCheckCacheRequestRepository = employmentCheckCacheRequestRepository;
        }

        public async Task<Data.Models.EmploymentCheck> GetLastEmploymentCheck(Guid correlationId)
        {
            var existingEmploymentCheck = await _employmentCheckRepository.GetEmploymentCheck(correlationId);

            return existingEmploymentCheck;
        }

        public void InsertEmploymentCheck(Data.Models.EmploymentCheck employmentCheck)
        {
            _employmentCheckRepository.Insert(employmentCheck);
        }

        public async Task<IList<Data.Models.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            IList<Data.Models.EmploymentCheck> employmentChecksBatch = null;

            var dbConnection = new DbConnection();

            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                             _applicationSettings,
                             _azureServiceTokenProvider))
            {
                await sqlConnection.OpenAsync();
                {
                    var transaction = sqlConnection.BeginTransaction();

                    try
                    {
                        // Get a batch of employment checks that do not already have a matching pending EmploymentCheckCacheRequest
                        var parameters = new DynamicParameters();
                        parameters.Add("@batchSize", _batchSize);

                        employmentChecksBatch = (await sqlConnection.QueryAsync<Data.Models.EmploymentCheck>(
                            sql: "SELECT TOP (@batchSize) " +
                                 "[Id], " +
                                 "[CorrelationId], " +
                                 "[CheckType], " +
                                 "[Uln], " +
                                 "[ApprenticeshipId], " +
                                 "[AccountId], " +
                                 "[MinDate], " +
                                 "[MaxDate], " +
                                 "[Employed], " +
                                 "[RequestCompletionStatus], " +
                                 "[CreatedOn], " +
                                 "[LastUpdatedOn] " +
                                 "FROM [Business].[EmploymentCheck] AEC " +
                                 "WHERE (AEC.RequestCompletionStatus IS NULL) " +
                                 "ORDER BY AEC.Id ",
                            param: parameters,
                            commandType: CommandType.Text,
                            transaction: transaction)).ToList();

                        foreach (var employmentCheck in employmentChecksBatch)
                        {
                            var parameter = new DynamicParameters();
                            parameter.Add("@Id", employmentCheck.Id, DbType.Int64);
                            parameter.Add("@requestCompletionStatus", ProcessingCompletionStatus.Started, DbType.Int16);
                            parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                            await sqlConnection.ExecuteAsync(
                                "UPDATE [Business].[EmploymentCheck] " +
                                "SET RequestCompletionStatus = @requestCompletionStatus, LastUpdatedOn = @lastUpdatedOn " +
                                "WHERE Id = @Id ",
                                parameter,
                                commandType: CommandType.Text,
                                transaction: transaction);
                        }

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback(); // rollback the whole batch rather than individual employment checks
                        _logger.LogError($"EmploymentCheckService.GetEmploymentChecksBatch(): ERROR: An error occurred reading the employment checks. Exception [{e}]");
                        throw;
                    }
                }
            }

            return employmentChecksBatch;
        }

        /// <summary>
        /// Creates an EmploymentCheckCacheRequest for each employment check in the given batch of employment checks
        /// </summary>
        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{nameof(EmploymentCheckService)}.CreateEmploymentCheckCacheRequests";
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));

            // Validate that the employmentCheckData lists have data
            var employmentCheckDataValidator = new EmploymentCheckDataValidator();
            var employmentCheckDataValidatorResult = await employmentCheckDataValidator.ValidateAsync(employmentCheckData);

            // EmploymentCheckData validation - failed
            // Log the validation errors
            if (!employmentCheckDataValidatorResult.IsValid)
            {
                foreach (var error in employmentCheckDataValidatorResult.Errors)
                {
                    _logger.LogError($"{thisMethodName}: ERROR - EmploymentCheckData: {error.ErrorMessage}");
                }

                return await Task.FromResult(new List<EmploymentCheckCacheRequest>());  // caller should check for an empty list of EmploymentCheckCacheRequests
            }

            // EmploymentCheckData validation - succeeded
            // Create an EmploymentCheckCacheRequest for each unique combination of Uln, National Insurance Number, PayeScheme, MinDate and MaxDate
            // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln, National Insurance Number, PayeScheme, MinDate and MaxDate
            var employmentCheckValidator = new EmploymentCheckValidator();
            IList<EmploymentCheckCacheRequest> employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
            foreach (var employmentCheck in employmentCheckData.EmploymentChecks)
            {
                // Validate the employmentCheck fields are not empty
                var employmentCheckValidatorResult = await employmentCheckValidator.ValidateAsync(employmentCheck);

                // EmploymentCheck validation - failed
                // Log the validation errors
                if (!employmentCheckValidatorResult.IsValid)
                {
                    foreach (var error in employmentCheckValidatorResult.Errors)
                    {
                        _logger.LogError($"{thisMethodName}: ERROR - EmploymentCheck: {error.ErrorMessage}");
                    }
                }

                // EmploymentCheckData validation - succeeded
                // Lookup the National Insurance Number for this apprentice in the employment check data
                var nationalInsuranceNumber = employmentCheckData.ApprenticeNiNumbers.FirstOrDefault(ninumber => ninumber.Uln == employmentCheck.Uln)?.NiNumber;
                if (string.IsNullOrEmpty(nationalInsuranceNumber))
                {
                    // No national insurance number found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (Nino not found).");

                    // Update the status of the 'stored' employment check with the request completion status so that we can see that this Nino was not found
                    employmentCheck.RequestCompletionStatus = (short)ProcessingCompletionStatus.ProcessingError_NinoNotFound;
                    try
                    {
                        await SaveEmploymentCheck(employmentCheck);
                    }
                    catch(Exception e)
                    {
                        _logger.LogError($"EmploymentCheckService.CreateEmploymentCheckCacheRequests(): ERROR: the EmploymentCheck repository Save() method threw an Exception during the check for missing ninos [{e}]");
                    }

                    continue;
                }

                // Lookup the PayeSchemes for this apprentice in the employment check data
                var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.FirstOrDefault(ps => ps.EmployerAccountId == employmentCheck.AccountId);
                if (employerPayeSchemes == null)
                {
                    // No paye schemes found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (PayeScheme not found).");

                    // Update the status of the 'stored' employment check with the request completion status so that we can see that this PayeScheme was not found
                    employmentCheck.RequestCompletionStatus = (short)ProcessingCompletionStatus.ProcessingError_PayeSchemeNotFound;
                    try
                    {
                        await SaveEmploymentCheck(employmentCheck);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"EmploymentCheckService.CreateEmploymentCheckCacheRequests(): ERROR: the EmploymentCheck repository Save() method threw an Exception during the check for missing paye schemes [{e}]");
                    }

                    continue;
                }

                // Create the individual EmploymentCheckCacheRequest combinations for each paye scheme
                foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                {
                    if (string.IsNullOrEmpty(payeScheme))
                    {
                        // An empty paye scheme so we're not able to do an employment check and will need to skip this
                        _logger.LogError($"{thisMethodName}: An empty PAYE scheme was found for apprentice Uln: [{employmentCheck.Uln}] accountId [{employmentCheck.AccountId}].");
                        continue;
                    }

                    var employmentCheckCacheRequest = new EmploymentCheckCacheRequest();
                    employmentCheckCacheRequest.ApprenticeEmploymentCheckId = employmentCheck.Id;
                    employmentCheckCacheRequest.CorrelationId = employmentCheck.CorrelationId;
                    employmentCheckCacheRequest.Nino = nationalInsuranceNumber;
                    employmentCheckCacheRequest.PayeScheme = payeScheme;
                    employmentCheckCacheRequest.MinDate = employmentCheck.MinDate;
                    employmentCheckCacheRequest.MaxDate = employmentCheck.MaxDate;

                    employmentCheckRequests.Add(employmentCheckCacheRequest);

                    // Store the individual EmploymentCheckCacheRequest combinations for each paye scheme
                    try
                    {
                        await _employmentCheckCacheRequestRepository.Save(employmentCheckCacheRequest);
                    }
                    catch(Exception e)
                    {
                        _logger.LogError($"EmploymentCheckService.CreateEmploymentCheckCacheRequests(): ERROR: the EmploymentCheckCashRequest repository Save() method threw an Exception during the storing of the EmploymentCheckCacheRequest [{e}]");
                    }
                }
            }

            return await Task.FromResult(employmentCheckRequests);
        }
        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest()
        {
            var dbConnection = new DbConnection();

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _applicationSettings,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            EmploymentCheckCacheRequest employmentCheckCacheRequest = null;
            await sqlConnection.OpenAsync();
            {
                var transaction = sqlConnection.BeginTransaction();
                try
                {
                    // Get a batch of employment checks that do not already have a matching pending EmploymentCheckCacheRequest
                    var parameters = new DynamicParameters();
                    parameters.Add("@batchSize", _batchSize);

                    employmentCheckCacheRequest = (await sqlConnection.QueryAsync<EmploymentCheckCacheRequest>(
                        sql: "SELECT    TOP(1) * " +
                             "FROM      [Cache].[EmploymentCheckCacheRequest] " +
                             "WHERE     (RequestCompletionStatus IS NULL)" +
                             "ORDER BY  Id",
                        commandType: CommandType.Text,
                        transaction: transaction)).FirstOrDefault();

                    // Set the RequestCompletionStatus to 'Started' so that this request doesn't get read the next time around
                    if (employmentCheckCacheRequest != null)
                    {
                        var parameter = new DynamicParameters();
                        parameter.Add("@Id", employmentCheckCacheRequest.Id, DbType.Int64);
                        parameter.Add("@requestCompletionStatus", ProcessingCompletionStatus.Started, DbType.Int16);
                        parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                        await sqlConnection.ExecuteAsync(
                            "UPDATE [Cache].[EmploymentCheckCacheRequest] " +
                            "SET    RequestCompletionStatus = @requestCompletionStatus, " +
                            "       LastUpdatedOn = @lastUpdatedOn " +
                            "WHERE  Id = @Id ",
                            parameter,
                            commandType: CommandType.Text,
                            transaction: transaction);
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return employmentCheckCacheRequest;
        }
        public async Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            await UpdateEmploymentCheckCacheRequest(employmentCheckCacheRequest);
            await UpdateEmploymentCheck(employmentCheckCacheRequest);
        }

        public async Task UpdateEmploymentCheckCacheRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            var dbConnection = new DbConnection();

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _applicationSettings,
                _azureServiceTokenProvider);

            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.OpenAsync();
            var parameter = new DynamicParameters();
            parameter.Add("@Id", employmentCheckCacheRequest.Id, DbType.Int64);
            parameter.Add("@employed", employmentCheckCacheRequest.Employed, DbType.Boolean);
            parameter.Add("@requestCompletionStatus", employmentCheckCacheRequest.RequestCompletionStatus,
                DbType.Int16);
            parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

            await sqlConnection.ExecuteAsync(
                "UPDATE [Cache].[EmploymentCheckCacheRequest] " +
                // TODO: setting the completion status after the initial setting of it to 'started' is to be done under story 183
                "SET Employed = @employed, LastUpdatedOn = @lastUpdatedOn " +
                "WHERE Id = @Id ",
                parameter,
                commandType: CommandType.Text);
        }
        public async Task UpdateEmploymentCheck(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            var dbConnection = new DbConnection();

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _applicationSettings,
                _azureServiceTokenProvider);

            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.OpenAsync();

            var parameter = new DynamicParameters();
            parameter.Add("@apprenticeEmploymentCheckId", employmentCheckCacheRequest.ApprenticeEmploymentCheckId, DbType.Int64);
            parameter.Add("@employed", employmentCheckCacheRequest.Employed, DbType.Boolean);
            parameter.Add("@requestCompletionStatus", employmentCheckCacheRequest.RequestCompletionStatus, DbType.Int16);
            parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

            await sqlConnection.ExecuteAsync(
                "UPDATE [Business].[EmploymentCheck] " +
                // TODO: setting the completion status after the initial setting of it to 'started' is to be done under story 183
                "SET Employed = @employed, LastUpdatedOn = @lastUpdatedOn " +
                "WHERE Id = @ApprenticeEmploymentCheckId AND (Employed IS NULL OR Employed = 0) ",
                parameter,
                commandType: CommandType.Text);
        }

        private async Task SaveEmploymentCheck(Data.Models.EmploymentCheck employmentCheck)
        {
            if (employmentCheck == null)
            {
                _logger.LogError($"LearnerService.Save(): ERROR: The employmentCheck model is null.");
                return;
            }

            // Temporary work-around try/catch for handling duplicate inserts until we switch to single message processing
            try
            {
                await _employmentCheckRepository.Save(employmentCheck);
            }
            catch
            {
                // No logging, we're not interested in storing errors about duplicates at the moment
            }
        }
    }
}