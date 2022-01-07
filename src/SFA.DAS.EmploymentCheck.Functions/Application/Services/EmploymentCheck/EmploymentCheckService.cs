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

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : IEmploymentCheckService
    {
        #region Private members
        private const string ThisClassName = "\n\nEmploymentCheckService";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly ILogger<IEmploymentCheckService> _logger;
        private readonly EmploymentCheckDataValidator _employmentCheckDataValidator;
        private readonly ApprenticeEmploymentCheckValidator _apprenticeEmploymentCheckValidator;
        private readonly EmploymentCheckCacheRequestValidator _employmentCheckRequestValidator;

        private const string AzureResource = "https://database.windows.net/"; // TODO: move to config
        private readonly string _connectionString = System.Environment.GetEnvironmentVariable($"EmploymentChecksConnectionString"); // TODO: move to config
        private readonly int _batchSize; // TODO: move to config
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        #endregion Private members

        #region Constructors
        /// <summary>
        /// The EmploymentCheckService contains the methods to read and save Employment Checks
        /// </summary>
        /// <param name="applicationSettings"></param>
        /// <param name="employmentCheckDbConfiguration"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="logger"></param>
        public EmploymentCheckService(
            ILogger<IEmploymentCheckService> logger,
            ApplicationSettings applicationSettings,                            // TODO: Replace this generic application setting
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            _logger = logger;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
            _employmentCheckDataValidator = new EmploymentCheckDataValidator();
            _apprenticeEmploymentCheckValidator = new ApprenticeEmploymentCheckValidator();
            _employmentCheckRequestValidator = new EmploymentCheckCacheRequestValidator();
        }
        #endregion Constructors

        #region GetEmploymentChecksBatch
        public async Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentChecksBatch()";
            IList<Models.EmploymentCheck> employmentChecksBatch = null;

            var dbConnection = new DbConnection();
            Guard.Against.Null(dbConnection, nameof(dbConnection));

            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _logger,
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider))
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));

                await sqlConnection.OpenAsync();
                {
                    try
                    {
                        _logger.LogInformation($"{thisMethodName}: Getting the EmploymentChecksBatch");

                        // Get a batch of employment checks that do not already have a matching pending EmploymentCheckCacheRequest
                        var parameters = new DynamicParameters();
                        parameters.Add("@batchSize", _batchSize);

                        employmentChecksBatch = (await sqlConnection.QueryAsync<Models.EmploymentCheck>(
                                sql: "SELECT TOP (@batchSize)" +
                                    "[Id], " +
                                    "[CorrelationId], " +
                                    "[CheckType], " +
                                    "[Uln], " +
                                    "[ApprenticeshipId], " +
                                    "[AccountId], " +
                                    "[MinDate], " +
                                    "[MaxDate], " +
                                    "[Employed], " +
                                    "[CreatedOn], " +
                                    "[LastUpdatedOn] " +
                                    "FROM [Business].[EmploymentCheck] AEC " +
                                    "WHERE AEC.Id > (SELECT ISNULL(MAX(ApprenticeEmploymentCheckId), 0) FROM [Cache].[EmploymentCheckCacheRequest]) " +
                                    "AND AEC.Id NOT IN (SELECT ApprenticeEmploymentCheckId FROM [Cache].[EmploymentCheckCacheRequest] WHERE (RequestCompletionStatus = 0 OR RequestCompletionStatus IS NULL)) " +
                                    "ORDER BY AEC.Id ",
                                param: parameters,
                                commandType: CommandType.Text)).ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the employmentChecksBatch failed - {ex.Message}. {ex.StackTrace}");
                    }
                }
            }

            Guard.Against.Null(employmentChecksBatch, nameof(employmentChecksBatch));
            return employmentChecksBatch;
        }
        #endregion GetEmploymentChecksBatch

        #region CreateEmploymentCheckCacheRequest
        /// <summary>
        /// Creates an EmploymentCheckCacheRequest for each employment check in the given batch of employment checks
        /// </summary>
        /// <returns>Task<IList<EmploymentCheckRequest>></returns>
        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{nameof(ThisClassName)}.CreateEmploymentCheckCacheRequests()";
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));

            IList<EmploymentCheckCacheRequest> employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
            ApprenticeEmploymentCheckValidator apprenticeEmploymentCheckValidator = new ApprenticeEmploymentCheckValidator();
            try
            {
                var employmentCheckValidatorResult = _employmentCheckDataValidator.Validate(employmentCheckData);
                Guard.Against.Null(employmentCheckValidatorResult, nameof(employmentCheckValidatorResult));

                if (employmentCheckValidatorResult.IsValid)
                {
                    // Create an EmploymentCheckCacheRequest for each combination of Uln, National Insurance Number and PayeScheme.
                    // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln and National Insurance Number
                    foreach (var employmentCheck in employmentCheckData.EmploymentChecks)
                    {
                        var validationResult = apprenticeEmploymentCheckValidator.Validate(employmentCheck);

                        if (validationResult != null && validationResult.IsValid)
                        {
                            // Lookup the National Insurance Number for this apprentice in the employment check data
                            var nationalInsuranceNumber = employmentCheckData.ApprenticeNiNumbers.Where(ninumber => ninumber.Uln == employmentCheck.Uln).FirstOrDefault().NiNumber;
                            if (string.IsNullOrEmpty(nationalInsuranceNumber))
                            {
                                // No national insurance number found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} No national insurance number found for apprentice Uln: [{employmentCheck.Uln}].");
                                continue;
                            }

                            // Lookup the National Insurance Number for this apprentice in the employment check data
                            var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.Where(ps => ps.EmployerAccountId == employmentCheck.AccountId).FirstOrDefault();
                            if (employerPayeSchemes == null)
                            {
                                // No paye schemes found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} No PAYE schemes found for apprentice Uln: [{employmentCheck.Uln}] accountId [{employmentCheck.AccountId}].");
                                continue;
                            }

                            foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                            {
                                if (string.IsNullOrEmpty(payeScheme))
                                {
                                    // An empty paye scheme so we're not able to do an employment check and will need to skip this
                                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} An empty PAYE scheme was found for apprentice Uln: [{employmentCheck.Uln}] accountId [{employmentCheck.AccountId}].");
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
                                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} EmploymentCheckRequest: {error.ErrorMessage}");
                            }
                        }
                    }
                }
                else
                {
                    foreach (var error in employmentCheckValidatorResult.Errors)
                    {
                        _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} EmploymentCheckMode {error.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(employmentCheckRequests);
        }
        #endregion CreateEmploymentCheckCacheRequest

        #region ProcessEmploymentCheckCacheRequest
        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest()
        {
            var thisMethodName = $"{ThisClassName}.ProcessEmploymentCheckCacheRequest()";

            EmploymentCheckCacheRequest employmentCheckCacheRequest = null;

            var dbConnection = new DbConnection();
            Guard.Against.Null(dbConnection, nameof(dbConnection));

            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _logger,
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider))
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));

                await sqlConnection.OpenAsync();
                {
                    try
                    {
                        _logger.LogInformation($"{thisMethodName}: Getting the EmploymentCheckCacheRequest");

                        employmentCheckCacheRequest = (await sqlConnection.QueryAsync<EmploymentCheckCacheRequest>(
                            sql:    "SELECT TOP(1)[Id] " +
                                    ",[ApprenticeEmploymentCheckId] " +
                                    ",[CorrelationId] " +
                                    ",[Nino] " +
                                    ",[PayeScheme] " +
                                    ",[MinDate] " +
                                    ",[MaxDate] " +
                                    ",[Employed] " +
                                    ",[RequestCompletionStatus] " +
                                    "FROM [Cache].[EmploymentCheckCacheRequest] " +
                                    "WHERE Employed IS NULL " +
                                    "ORDER BY Id",
                    commandType: CommandType.Text)).FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the EmploymentCheckCacheRequest failed - {ex.Message}. {ex.StackTrace}");
                    }
                }
            }

            return employmentCheckCacheRequest;
        }
        #endregion ProcessEmploymentCheckCacheRequest

        #region StoreEmploymentCacheRequest
        private async Task StoreEmploymentCheckCacheRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var thisMethodName = $"{nameof(ThisClassName)}.StoreEmploymentCheckCacheRequest()";

            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            try
            {
                var dbConnection = new DbConnection();
                Guard.Against.Null(dbConnection, nameof(dbConnection));

                await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                    _logger,
                    _connectionString,
                    AzureResource,
                    _azureServiceTokenProvider))
                {

                    if (sqlConnection != null)
                    {
                        await sqlConnection.OpenAsync();
                        {
                            try
                            {
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
                                    "INSERT [SFA.DAS.EmploymentCheck.Database].[Cache].[EmploymentCheckCacheRequest] " +
                                    "       ( ApprenticeEmploymentCheckId,  CorrelationId,  Nino,  PayeScheme,  MinDate,  MaxDate,  Employed,  CreatedOn) " +
                                    "VALUES (@ApprenticeEmploymentCheckId, @correlationId, @Nino, @payeScheme, @minDate, @maxDate, @employed, @createdOn) ",
                                    parameter,
                                    commandType: CommandType.Text);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Sql Connection is NULL");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
        #endregion StoreEmploymentCacheRequest

        #region StoreEmploymentCheckResult
        public async Task StoreEmploymentCheckResult(
             EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var thisMethodName = $"{nameof(ThisClassName)}.StoreEmploymentCheckResult()";

            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            try
            {
                await UpdateEmploymentCheckCacheRequest(employmentCheckCacheRequest);
                await UpdateEmploymentCheck(employmentCheckCacheRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
        #endregion StoreEmploymentCheckResult

        #region UpdateEmploymentCacheRequest
        public async Task UpdateEmploymentCheckCacheRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var thisMethodName = $"{nameof(ThisClassName)}.UpdateEmploymentCheckCacheRequest()";

            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            try
            {
                var dbConnection = new DbConnection();
                Guard.Against.Null(dbConnection, nameof(dbConnection));

                await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                    _logger,
                    _connectionString,
                    AzureResource,
                    _azureServiceTokenProvider))
                {

                    if (sqlConnection != null)
                    {
                        await sqlConnection.OpenAsync();
                        {
                            try
                            {
                                var parameter = new DynamicParameters();
                                parameter.Add("@Id", employmentCheckCacheRequest.Id, DbType.Int64);
                                parameter.Add("@employed", employmentCheckCacheRequest.Employed, DbType.Boolean);
                                parameter.Add("@requestCompletionStatus", employmentCheckCacheRequest.RequestCompletionStatus, DbType.Int16);
                                parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                                await sqlConnection.ExecuteAsync(
                                    "UPDATE [SFA.DAS.EmploymentCheck.Database].[Cache].[EmploymentCheckCacheRequest] " +
                                    "SET Employed = @employed, RequestCompletionStatus = @requestCompletionStatus, LastUpdatedOn = @lastUpdatedOn " +
                                    "WHERE Id = @Id ",
                                    parameter,
                                    commandType: CommandType.Text);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Sql Connection is NULL");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
        #endregion UpdateEmploymentCacheRequest

        #region UpdateEmploymentCheck
        public async Task UpdateEmploymentCheck(
             EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var thisMethodName = $"{nameof(ThisClassName)}.UpdateEmploymentCheck()";

            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            try
            {
                var dbConnection = new DbConnection();
                Guard.Against.Null(dbConnection, nameof(dbConnection));

                await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                    _logger,
                    _connectionString,
                    AzureResource,
                    _azureServiceTokenProvider))
                {

                    if (sqlConnection != null)
                    {
                        await sqlConnection.OpenAsync();
                        {
                            try
                            {
                                var parameter = new DynamicParameters();
                                parameter.Add("@apprenticeEmploymentCheckId", employmentCheckCacheRequest.ApprenticeEmploymentCheckId, DbType.Int64);
                                parameter.Add("@employed", employmentCheckCacheRequest.Employed, DbType.Boolean);
                                parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                                await sqlConnection.ExecuteAsync(
                                    "UPDATE [SFA.DAS.EmploymentCheck.Database].[Business].[EmploymentCheck] " +
                                    "SET Employed = @employed, LastUpdatedOn = @lastUpdatedOn " +
                                    "WHERE Id = @ApprenticeEmploymentCheckId AND (Employed IS NULL OR Employed = 0) ",
                                    parameter,
                                    commandType: CommandType.Text);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Sql Connection is NULL");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
        #endregion UpdateEmploymentCheck

        #region SeedEmploymentCheckDatabaseTableTestData
        public async Task SeedEmploymentCheckDatabaseTableTestData()
        {
            var thisMethodName = $"{ThisClassName}.SeedEmploymentCheckDatabaseTableTestData()";

            try
            {
                var dbConnection = new DbConnection(); // TODO: Move to startup DI
                if (dbConnection != null)
                {
                    await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                    _logger,
                    _connectionString,
                    AzureResource,
                    _azureServiceTokenProvider))
                    {
                        if (sqlConnection != null)
                        {
                            await sqlConnection.OpenAsync();

                            var i = 0;
                            foreach (var input in StubEmploymentCheckMessageData)
                            {
                                i++;
                                var now = DateTime.Now;
                                var parameters = new DynamicParameters();
                                parameters.Add("@correlationId", new Guid(), DbType.Guid);
                                parameters.Add("@checkType", "StartDate+60", DbType.String);
                                parameters.Add("@uln", 1000000000 + i, DbType.Int64);
                                parameters.Add("@apprenticeshipId", i, DbType.Int64);
                                parameters.Add("@accountId", i, DbType.Int64);
                                parameters.Add("@minDate", input.MinDate, DbType.DateTime);
                                parameters.Add("@maxDate", input.MaxDate, DbType.DateTime);
                                parameters.Add("@employed", false, DbType.Boolean);
                                parameters.Add("@lastUpdatedOn", now, DbType.DateTime);
                                parameters.Add("@createdOn", now, DbType.DateTime);


                                await sqlConnection.ExecuteAsync(
                                    "INSERT [Business].[ApprenticeEmploymentCheck] (" +
                                    "CorrelationId, " +
                                    "CheckType, " +
                                    "Uln, " +
                                    "ApprenticeshipId, " +
                                    "AccountId, " +
                                    "MinDate, " +
                                    "MaxDate, " +
                                    "Employed, " +
                                    "LastUpdatedOn, " +
                                    "CreatedOn) " +
                                    "VALUES (" +
                                    "@correlationId, " +
                                    "@checkType, " +
                                    "@uln, " +
                                    "@apprenticeshipId, " +
                                    "@accountId, " +
                                    "@minDate, " +
                                    "@maxDate, " +
                                    "@employed, " +
                                    "@lastUpdatedOn, " +
                                    "@createdOn) ",
                                    commandType: CommandType.Text,
                                    param: parameters);
                            }
                        }
                        else
                        {
                            _logger.LogInformation(
                                $"\n\n{DateTime.UtcNow} {thisMethodName}: *** ERROR ***: Creation of SQL Connection for the Employment Check Database failed.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
        #endregion SeedEmploymentCheckDatabaseTableTestData

        #region StubEmploymentCheckMessageData
        public static EmploymentCheckCacheRequest[] StubEmploymentCheckMessageData => new[] {
             new EmploymentCheckCacheRequest()
             {
                 PayeScheme = "123/AB12345",
                 Nino = "SC111111A",
                 MinDate = new DateTime(2010, 01, 01),
                 MaxDate = new DateTime(2018, 01, 01)
             },
             new EmploymentCheckCacheRequest()
             {
                 PayeScheme = "840/MODES17",
                 Nino = "SC111111A",
                 MinDate = new DateTime(2010, 01, 01),
                 MaxDate = new DateTime(2018, 01, 01)
             },
             new EmploymentCheckCacheRequest()
             {
                 PayeScheme = "840/MODES17",
                 Nino = "AA123456C",
                 MinDate = new DateTime(2010, 01, 01),
                 MaxDate = new DateTime(2018, 01, 01)
             },
             new EmploymentCheckCacheRequest()
             {
                 PayeScheme = "111/AA00001",
                 Nino = "AA123456C",
                 MinDate = new DateTime(2010, 01, 01),
                 MaxDate = new DateTime(2018, 01, 01)
             },
             new EmploymentCheckCacheRequest()
             {
                 PayeScheme = "840/HZ00064",
                 Nino = "AS960509A",
                 MinDate = new DateTime(2010, 01, 01),
                 MaxDate = new DateTime(2018, 01, 01)
             },
             new EmploymentCheckCacheRequest()
             {
                 PayeScheme = "923/EZ00059",
                 Nino = "PR555555A",
                 MinDate = new DateTime(2010, 01, 01),
                 MaxDate = new DateTime(2018, 01, 01)
             }
        };
        #endregion StubEmploymentCheckMessageData
    }
}