using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : IEmploymentCheckService
    {
        private readonly ILogger<IEmploymentCheckService> _logger;
        private readonly string _connectionString;
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly IEmploymentCheckCacheRequestRepository _employmentCheckCashRequestRepository;

        public EmploymentCheckService(
            ILogger<IEmploymentCheckService> logger,
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider,
            IEmploymentCheckRepository employmentCheckRepository,
            IEmploymentCheckCacheRequestRepository employmentCheckCashRequestRepository
        )
        {
            _logger = logger;
            _connectionString = applicationSettings.DbConnectionString;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
            _employmentCheckCashRequestRepository = employmentCheckCashRequestRepository;
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
                        transaction.Rollback();
                        _logger.LogError($"EmploymentCheckService.GetEmploymentChecksBatch(): ERROR: An error occurred reading the employment checks. Exception [{e}]");
                        throw;
                    }
                }
            }

            return employmentChecksBatch;
        }

        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{nameof(EmploymentCheckService)}.CreateEmploymentCheckCacheRequests";
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));

            var employmentCheckDataValidator = new EmploymentCheckDataValidator();
            var employmentCheckDataValidatorResult = await employmentCheckDataValidator.ValidateAsync(employmentCheckData);

            if (!employmentCheckDataValidatorResult.IsValid)
            {
                foreach (var error in employmentCheckDataValidatorResult.Errors)
                {
                    _logger.LogError($"{thisMethodName}: ERROR - EmploymentCheckData: {error.ErrorMessage}");
                }

                return await Task.FromResult(new List<EmploymentCheckCacheRequest>());
            }

            var employmentCheckValidator = new EmploymentCheckValidator();
            IList<EmploymentCheckCacheRequest> employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
            foreach (var employmentCheck in employmentCheckData.EmploymentChecks)
            {
                var employmentCheckValidatorResult = await employmentCheckValidator.ValidateAsync(employmentCheck);
                if (!employmentCheckValidatorResult.IsValid)
                {
                    foreach (var error in employmentCheckValidatorResult.Errors)
                    {
                        _logger.LogError($"{thisMethodName}: ERROR - EmploymentCheck: {error.ErrorMessage}");
                    }
                }

                var nationalInsuranceNumber = employmentCheckData.ApprenticeNiNumbers.FirstOrDefault(ninumber => ninumber.Uln == employmentCheck.Uln)?.NiNumber;
                if (string.IsNullOrEmpty(nationalInsuranceNumber))
                {
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (Nino not found).");
                    continue;
                }

                var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.FirstOrDefault(ps => ps.EmployerAccountId == employmentCheck.AccountId);
                if (employerPayeSchemes == null)
                {
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (PayeScheme not found).");
                    continue;
                }

                foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                {
                    if (string.IsNullOrEmpty(payeScheme))
                    {
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

                    await _employmentCheckCashRequestRepository.Save(employmentCheckCacheRequest);
                }
            }

            return await Task.FromResult(employmentCheckRequests);
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
                    var parameters = new DynamicParameters();
                    parameters.Add("@batchSize", _batchSize);

                    employmentCheckCacheRequest = (await sqlConnection.QueryAsync<EmploymentCheckCacheRequest>(
                        sql: "SELECT    TOP(1) * " +
                             "FROM      [Cache].[EmploymentCheckCacheRequest] " +
                             "WHERE     (RequestCompletionStatus IS NULL)" +
                             "ORDER BY  Id",
                        commandType: CommandType.Text,
                        transaction: transaction)).FirstOrDefault();

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

            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);

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

        public async Task UpdateEmploymentCheck(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            if(employmentCheckCacheRequest.RequestCompletionStatus != null)
            {
                employmentCheckCacheRequest.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
            }

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider))
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

        public async Task UpdateRelatedRequests(EmploymentCheckCacheRequest request)
        {
            await _employmentCheckCashRequestRepository.SetReleatedRequestsRequestCompletionStatus(request, ProcessingCompletionStatus.Skipped);
        }
    }
}