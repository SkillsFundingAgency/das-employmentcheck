using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Repositories;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : IEmploymentCheckService
    {
        private readonly ILogger<IEmploymentCheckService> _logger;
        private readonly string _connectionString;
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly IEmploymentCheckRepository _employmentCheckRepository;
        private readonly IEmploymentCheckCacheRequestRepository _employmentCheckCashRequestRepository;

        /// <summary>
        /// The EmploymentCheckService contains the methods to read and save Employment Checks
        /// </summary>
        /// <param name="applicationSettings"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="logger"></param>
        public EmploymentCheckService(
            ILogger<IEmploymentCheckService> logger,
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider,
            IEmploymentCheckRepository employmentCheckRepositoryrepository,
            IEmploymentCheckCacheRequestRepository employmentCheckCashRequestRepositoryrepository
        )
        {
            _logger = logger;
            _connectionString = applicationSettings.DbConnectionString;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
            _employmentCheckRepository = employmentCheckRepositoryrepository;
            _employmentCheckCashRequestRepository = employmentCheckCashRequestRepositoryrepository;
        }

        public async Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            IList<Models.EmploymentCheck> employmentChecksBatch = null;

            var dbConnection = new DbConnection();

            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
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

                        employmentChecksBatch = (await sqlConnection.QueryAsync<Models.EmploymentCheck>(
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

                        // Set the RequestCompletionStatus to 'Started' on the batch that has just read so that
                        // these employment checks don't get read next time around if there's an exception due to the RequestCompletionStatus still being set to null
                        if (employmentChecksBatch != null && employmentChecksBatch.Count > 0)
                        {
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

                    continue;
                }

                // Lookup the PayeSchemes for this apprentice in the employment check data
                var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.FirstOrDefault(ps => ps.EmployerAccountId == employmentCheck.AccountId);
                if (employerPayeSchemes == null)
                {
                    // No paye schemes found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (PayeScheme not found).");

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
                    employmentCheckCacheRequest.CreatedOn = DateTime.Now;
                    employmentCheckCacheRequest.LastUpdatedOn = DateTime.Now;

                    employmentCheckRequests.Add(employmentCheckCacheRequest);

                    // Store the individual EmploymentCheckCacheRequest combinations for each paye scheme
                    try
                    {
                        await _employmentCheckCashRequestRepository.Save(employmentCheckCacheRequest);
                    }
                    catch(Exception e)
                    {
                        _logger.LogError($"EmploymentCheckService.CreateEmploymentCheckCacheRequests(): ERROR: the EmploymentCheckCashRequest repository Save() method threw an Exception during the storing of the EmploymentCheckCacheRequest [{e}]");
                    }
                }
            }

            return await Task.FromResult(employmentCheckRequests);
        }

        public async Task<IList<EmploymentCheckCacheRequest>> SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus(
            Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet
        )
        {
            return await _employmentCheckCashRequestRepository.SetReleatedRequestsRequestCompletionStatus(employmentCheckCacheRequestAndStatusToSet);
        }


        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest()
        {
            var dbConnection = new DbConnection();

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
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

            if (employmentCheckCacheRequest.RequestCompletionStatus == (short)ProcessingCompletionStatus.Started)
            {
                employmentCheckCacheRequest.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
            }
            await UpdateEmploymentCheckCacheRequest(employmentCheckCacheRequest);
            await UpdateEmploymentCheck(employmentCheckCacheRequest);
        }

        public async Task UpdateEmploymentCheckCacheRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));

                await sqlConnection.OpenAsync();
                var parameter = new DynamicParameters();
                parameter.Add("@Id", employmentCheckCacheRequest.Id, DbType.Int64);
                parameter.Add("@employed", employmentCheckCacheRequest.Employed, DbType.Boolean);
                parameter.Add("@requestCompletionStatus", employmentCheckCacheRequest.RequestCompletionStatus, DbType.Int16);
                parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                await sqlConnection.ExecuteAsync(
                    "UPDATE [Cache].[EmploymentCheckCacheRequest] " +
                    "SET Employed = @employed, requestCompletionStatus = @requestCompletionStatus, LastUpdatedOn = @lastUpdatedOn " +
                    "WHERE Id = @Id ",
                    parameter,
                    commandType: CommandType.Text);
            }
        }

        public async Task UpdateEmploymentCheck(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            if (employmentCheckCacheRequest.RequestCompletionStatus != null)
            {
                employmentCheckCacheRequest.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
            }

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();

                var parameter = new DynamicParameters();
                parameter.Add("@apprenticeEmploymentCheckId", employmentCheckCacheRequest.ApprenticeEmploymentCheckId, DbType.Int64);
                parameter.Add("@employed", employmentCheckCacheRequest.Employed, DbType.Boolean);
                parameter.Add("@requestCompletionStatus", employmentCheckCacheRequest.RequestCompletionStatus, DbType.Int16);
                parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                await sqlConnection.ExecuteAsync(
                    "UPDATE [Business].[EmploymentCheck] " +
                    "SET Employed = @employed, RequestCompletionStatus = @requestCompletionStatus, LastUpdatedOn = @lastUpdatedOn " +
                    "WHERE Id = @ApprenticeEmploymentCheckId AND (Employed IS NULL OR Employed = 0) ",
                    parameter,
                    commandType: CommandType.Text);
            }
        }

        private async Task SaveEmploymentCheck(Models.EmploymentCheck employmentCheck)
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