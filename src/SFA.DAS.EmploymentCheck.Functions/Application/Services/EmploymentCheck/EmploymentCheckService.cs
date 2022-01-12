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

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : IEmploymentCheckService
    {
        private readonly ILogger<IEmploymentCheckService> _logger;
        private readonly EmploymentCheckDataValidator _employmentCheckDataValidator;

        private const string AzureResource = "https://database.windows.net/";
        private readonly string _connectionString;
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        /// <summary>
        /// The EmploymentCheckService contains the methods to read and save Employment Checks
        /// </summary>
        /// <param name="applicationSettings"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="logger"></param>
        public EmploymentCheckService(
            ILogger<IEmploymentCheckService> logger,
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            _logger = logger;
            _connectionString = applicationSettings.DbConnectionString;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
            _employmentCheckDataValidator = new EmploymentCheckDataValidator();
        }

        #region GetEmploymentChecksBatch
        public async Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            IList<Models.EmploymentCheck> employmentChecksBatch = null;

            var dbConnection = new DbConnection();

            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                AzureResource,
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
                                    "WHERE AEC.RequestCompletionStatus IS NULL OR AEC.RequestCompletionStatus = 0 " +
                                    "AND  AEC.Id >          ( " +
                                    "                           SELECT  ISNULL(MAX(ApprenticeEmploymentCheckId), 0) " +
                                    "                           FROM    [Cache].[EmploymentCheckCacheRequest] ECCR " +
                                    "                           WHERE   ECCR.RequestCompletionStatus IS NULL " +
                                    "                       ) " +
                                    "AND    AEC.Id NOT IN   (" +
                                    "                           SELECT  ISNULL(ApprenticeEmploymentCheckId, 0) " +
                                    "                           FROM    [Cache].[EmploymentCheckCacheRequest] " +
                                    "                           WHERE   (AEC.RequestCompletionStatus IS NULL OR AEC.RequestCompletionStatus = 0) " +
                                    "                       ) " +
                                    "ORDER BY AEC.Id ",
                                param: parameters,
                                commandType: CommandType.Text,
                                transaction: transaction)).ToList();

                        // Set the RequestCompletionStatus to 'HasBeenChecked' on the batch that has just read so that
                        // these employment checks don't get read next time around if there's an exception due to the RequestCompletionStatus still being set to null
                        if (employmentChecksBatch != null && employmentChecksBatch.Count > 0)
                        {
                            foreach (var employmentCheck in employmentChecksBatch)
                            {
                                var parameter = new DynamicParameters();
                                parameter.Add("@Id", employmentCheck.Id, DbType.Int64);
                                parameter.Add("@requestCompletionStatus", ProcessingCompletionStatus.HasBeenChecked, DbType.Int16);  // Set to to mimick the behaviour of the Dec release which had the 'HasBeenChecked'
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
                    catch (Exception)
                    {
                        transaction.Rollback(); // rollback the whole batch rather than individual employment checks
                        throw;
                    }
                }
            }

            return employmentChecksBatch;
        }
        #endregion GetEmploymentChecksBatch

        #region CreateEmploymentCheckCacheRequest
        /// <summary>
        /// Creates an EmploymentCheckCacheRequest for each employment check in the given batch of employment checks
        /// </summary>
        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{nameof(EmploymentCheckService)}.CreateEmploymentCheckCacheRequests";
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));

            IList<EmploymentCheckCacheRequest> employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
            var apprenticeEmploymentCheckValidator = new ApprenticeEmploymentCheckValidator();
                var employmentCheckValidatorResult = _employmentCheckDataValidator.Validate(employmentCheckData);
                Guard.Against.Null(employmentCheckValidatorResult, nameof(employmentCheckValidatorResult));

            if (employmentCheckValidatorResult.IsValid)
            {
                // Create an EmploymentCheckCacheRequest for each combination of Uln, National Insurance Number and PayeScheme.
                // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln and National Insurance Number
                foreach (var employmentCheck in employmentCheckData.EmploymentChecks)
                {
                    var validationResult = await apprenticeEmploymentCheckValidator.ValidateAsync(employmentCheck);

                    if (validationResult is {IsValid: true})
                    {
                        // Lookup the National Insurance Number for this apprentice in the employment check data
                        var nationalInsuranceNumber = employmentCheckData.ApprenticeNiNumbers.FirstOrDefault(ninumber => ninumber.Uln == employmentCheck.Uln)?.NiNumber;
                        if (string.IsNullOrEmpty(nationalInsuranceNumber))
                        {
                            // No national insurance number found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                            _logger.LogError($"{thisMethodName}: No national insurance number found for apprentice Uln: [{employmentCheck.Uln}].");
                            continue;
                        }

                        // Lookup the National Insurance Number for this apprentice in the employment check data
                        var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.FirstOrDefault(ps => ps.EmployerAccountId == employmentCheck.AccountId);
                        if (employerPayeSchemes == null)
                        {
                            // No paye schemes found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                            _logger.LogError($"{thisMethodName}: No PAYE schemes found for apprentice Uln: [{employmentCheck.Uln}] accountId [{employmentCheck.AccountId}].");
                            continue;
                        }

                        foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                        {
                            if (string.IsNullOrEmpty(payeScheme))
                            {
                                // An empty paye scheme so we're not able to do an employment check and will need to skip this
                                _logger.LogError($"{thisMethodName}: An empty PAYE scheme was found for apprentice Uln: [{employmentCheck.Uln}] accountId [{employmentCheck.AccountId}].");
                                continue;
                            }

                            // Create the individual EmploymentCheckCacheRequest combinations for each paye scheme
                            var employmentCheckCacheRequest = new EmploymentCheckCacheRequest();
                            employmentCheckCacheRequest.ApprenticeEmploymentCheckId = employmentCheck.Id;
                            employmentCheckCacheRequest.CorrelationId = employmentCheck.CorrelationId;
                            employmentCheckCacheRequest.Nino = nationalInsuranceNumber;
                            employmentCheckCacheRequest.PayeScheme = payeScheme;
                            employmentCheckCacheRequest.MinDate = employmentCheck.MinDate;
                            employmentCheckCacheRequest.MaxDate = employmentCheck.MaxDate;

                            // Store the individual EmploymentCheckCacheRequest combinations for each paye scheme
                            await StoreEmploymentCheckCacheRequest(employmentCheckCacheRequest);
                        }
                    }
                    else
                    {
                        foreach (var error in validationResult.Errors)
                        {
                            _logger.LogError($"{thisMethodName}: EmploymentCheckRequest: {error.ErrorMessage}");
                        }
                    }
                }
            }
            else
            {
                foreach (var error in employmentCheckValidatorResult.Errors)
                {
                    _logger.LogError($"{thisMethodName}: EmploymentCheckMode {error.ErrorMessage}");
                }
            }

            return await Task.FromResult(employmentCheckRequests);
        }
        #endregion CreateEmploymentCheckCacheRequest

        #region GetEmploymentCheckCacheRequest
        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest()
        {
            var dbConnection = new DbConnection();

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.OpenAsync();

            var employmentCheckCacheRequest = (await sqlConnection.QueryAsync<EmploymentCheckCacheRequest>(
                sql: "SELECT TOP(1)[Id] " +
                     ",[ApprenticeEmploymentCheckId] " +
                     ",[CorrelationId] " +
                     ",[Nino] " +
                     ",[PayeScheme] " +
                     ",[MinDate] " +
                     ",[MaxDate] " +
                     ",[Employed] " +
                     ",[RequestCompletionStatus] " +
                     "FROM [Cache].[EmploymentCheckCacheRequest] " +
                     "WHERE (RequestCompletionStatus IS NULL OR RequestCompletionStatus = 0)" +
                     "ORDER BY Id",
                commandType: CommandType.Text)).FirstOrDefault();

            return employmentCheckCacheRequest;
        }
        #endregion GetEmploymentCheckCacheRequest

        #region StoreEmploymentCacheRequest

        private async Task StoreEmploymentCheckCacheRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            var dbConnection = new DbConnection();

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.OpenAsync();

            var parameter = new DynamicParameters();
            parameter.Add("@ApprenticeEmploymentCheckId", employmentCheckCacheRequest.ApprenticeEmploymentCheckId, DbType.Int64);
            parameter.Add("@correlationId", employmentCheckCacheRequest.CorrelationId, DbType.Guid);
            parameter.Add("@Nino", employmentCheckCacheRequest.Nino, DbType.String);
            parameter.Add("@payeScheme", employmentCheckCacheRequest.PayeScheme, DbType.String);
            parameter.Add("@minDate", employmentCheckCacheRequest.MinDate, DbType.DateTime);
            parameter.Add("@maxDate", employmentCheckCacheRequest.MaxDate, DbType.DateTime);
            parameter.Add("@employed", employmentCheckCacheRequest.Employed, DbType.Boolean);
            parameter.Add("@createdOn", DateTime.Now, DbType.DateTime);

            await sqlConnection.ExecuteAsync(
                "INSERT [Cache].[EmploymentCheckCacheRequest] " +
                "       ( ApprenticeEmploymentCheckId,  CorrelationId,  Nino,  PayeScheme,  MinDate,  MaxDate,  Employed,  CreatedOn) " +
                "VALUES (@ApprenticeEmploymentCheckId, @correlationId, @Nino, @payeScheme, @minDate, @maxDate, @employed, @createdOn) ",
                parameter,
                commandType: CommandType.Text);
        }

        #endregion StoreEmploymentCacheRequest

        #region StoreEmploymentCheckResult

        public async Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            await UpdateEmploymentCheckCacheRequest(employmentCheckCacheRequest);
            await UpdateEmploymentCheck(employmentCheckCacheRequest);

        }

        #endregion StoreEmploymentCheckResult

        #region UpdateEmploymentCacheRequest

        public async Task UpdateEmploymentCheckCacheRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            var dbConnection = new DbConnection();

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                AzureResource,
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
                "SET Employed = @employed, RequestCompletionStatus = @requestCompletionStatus, LastUpdatedOn = @lastUpdatedOn " +
                "WHERE Id = @Id ",
                parameter,
                commandType: CommandType.Text);
        }

        #endregion UpdateEmploymentCacheRequest

        #region UpdateEmploymentCheck

        public async Task UpdateEmploymentCheck(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            var dbConnection = new DbConnection();

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                AzureResource,
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
                "SET Employed = @employed, RequestCompletionStatus = @requestCompletionStatus, LastUpdatedOn = @lastUpdatedOn " +
                "WHERE Id = @ApprenticeEmploymentCheckId AND (Employed IS NULL OR Employed = 0) ",
                parameter,
                commandType: CommandType.Text);
        }

        #endregion UpdateEmploymentCheck
    }
}