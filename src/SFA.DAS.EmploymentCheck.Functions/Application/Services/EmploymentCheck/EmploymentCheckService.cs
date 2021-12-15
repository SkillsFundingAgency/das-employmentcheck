using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Application.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : IEmploymentCheckService
    {
        private const string ThisClassName = "\n\nEmploymentCheckService";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private ILogger<IEmploymentCheckService> _logger;
        private EmploymentCheckRepository _employmentCheckRepository; // TODO: Move to startup DI

        // TODO: The service doesn't need to know about all this db stuff, it needs to be moved to a data access/repository
        private const string AzureResource = "https://database.windows.net/"; // TODO: move to config
        private readonly string _connectionString = System.Environment.GetEnvironmentVariable($"EmploymentChecksConnectionString"); // TODO: move to config
        private readonly int _batchSize; // TODO: move to config
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

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
            _employmentCheckRepository = new EmploymentCheckRepository(_logger, _connectionString, AzureResource); // TODO: Move to startup DI
        }

        /// <summary>
        /// Gets a batch of the employment checks from the Employment Check database
        /// </summary>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        public async Task<IList<EmploymentCheckModel>> GetEmploymentChecksBatch(
            long employmentCheckLastHighestBatchId)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentChecksBatch()";

            IList<EmploymentCheckModel> EmploymentCheckModels = null;
            try
            {
                // Get the Id of the last item in the previous batch
                employmentCheckLastHighestBatchId = await _employmentCheckRepository.GetEmploymentCheckLastHighestBatchId(null, null);

                // Get a batch of records starting from the last highest id
                EmploymentCheckModels = await _employmentCheckRepository.GetEmploymentCheckBatchById(employmentCheckLastHighestBatchId, _batchSize);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return EmploymentCheckModels;
        }

        //public async Task<IList<EmploymentCheckModel>> GetEmploymentChecksBatch(long employmentCheckLastHighestBatchId)
        //{
        //    var thisMethodName = $"{ThisClassName}.GetEmploymentChecksBatch()";

        //    IList<EmploymentCheckModel> EmploymentCheckModels = null;
        //    try
        //    {
        //        var dbConnection = new DbConnection(); // TODO: Move to startup DI
        //        if (dbConnection != null)
        //        {
        //            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
        //               _logger,
        //               _connectionString,
        //               AzureResource,
        //               _azureServiceTokenProvider))
        //            {
        //                if (sqlConnection != null)
        //                {
        //                    await sqlConnection.OpenAsync();

        //                    // Get the Id of the last item in the previous batch
        //                    employmentCheckLastHighestBatchId = await GetEmploymentCheckLastHighestBatchId(_logger, sqlConnection, null);

        //                    // Get a batch of records starting from the last highest id
        //                    EmploymentCheckModels = await GetEmploymentCheckBatch(_logger, sqlConnection, _batchSize, employmentCheckLastHighestBatchId);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return EmploymentCheckModels;
        //}


        //private async Task<IList<EmploymentCheckModel>> GetEmploymentCheckBatch(
        //    ILogger logger,
        //    SqlConnection sqlConnection,
        //    int batchSize,
        //    long employmentCheckLastHighestBatchId)
        //{
        //    var thisMethodName = $"{ThisClassName}.GetEmploymentCheckBatch()";

        //    IEnumerable<EmploymentCheckModel> employmentChecksResult = null;
        //    IList<EmploymentCheckModel> employmentCheckModels = null;
        //    try
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@batchSize", batchSize);
        //        parameters.Add("@employmentCheckLastHighestBatchId", employmentCheckLastHighestBatchId);

        //        // Get the next batch (or first batch if no previous batch) where the Id is greater than the stored employmentCheckLastHighestBatchId
        //        employmentChecksResult = await sqlConnection.QueryAsync<EmploymentCheckModel>(
        //        sql: "SELECT TOP (@batchSize) " +
        //            "Id, " +
        //            "CorrelationId, " +
        //            "CheckType, " +
        //            "Uln, " +
        //            "ApprenticeshipId, " +
        //            "AccountId, " +
        //            "MinDate, " +
        //            "MaxDate, " +
        //            "Employed, " +
        //            "CreatedOn, " +
        //            "LastUpdated " +
        //            "FROM [Business].[EmploymentCheck] " +
        //            "WHERE Id > @employmentCheckLastHighestBatchId " +
        //            "ORDER BY Id",
        //            param: parameters,
        //            commandType: CommandType.Text);

        //        // Check we got some results
        //        if (employmentChecksResult != null &&
        //            employmentChecksResult.Any())
        //        {
        //            logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: Database query returned [{employmentChecksResult.Count()}] employment checks.");

        //            // ... and return the results
        //            employmentCheckModels = employmentChecksResult.Select(aec => new EmploymentCheckModel(
        //                aec.Id,
        //                aec.CorrelationId,
        //                aec.CheckType,
        //                aec.Uln,
        //                aec.ApprenticeshipId,
        //                aec.AccountId,
        //                aec.MinDate,
        //                aec.MaxDate,
        //                aec.Employed,
        //                aec.LastUpdated,
        //                aec.CreatedOn)).ToList();
        //        }
        //        else
        //        {
        //            logger.LogError($"{thisMethodName}: Database query returned [0] employment checks.");
        //            employmentCheckModels = new List<EmploymentCheckModel>(); // return an empty list rather than null
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the employmentCheckLastHighestBatchId failed - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return employmentCheckModels;
        //}

        /// <summary>
        /// Creates an EmploymentCheckCacheRequest for each employment check in the given list of employment checks
        /// </summary>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            IList<EmploymentCheckModel> employmentCheckModels)
        {
            var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckCacheRequests()";

            IList<EmploymentCheckCacheRequest> employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest>();
            EmploymentCheckModelValidator employmentCheckModelValidator = new EmploymentCheckModelValidator();
            try
            {
                if (employmentCheckModels != null && employmentCheckModels.Any())
                {
                    // Create the individual requests
                    foreach (var employmentCheckModel in employmentCheckModels)
                    {
                        var validationResult = employmentCheckModelValidator.Validate(employmentCheckModel);

                        if (validationResult != null &&
                            validationResult.IsValid)
                        {
                            var employmentCheckCacheRequest = new EmploymentCheckCacheRequest();

                            employmentCheckCacheRequest.EmploymentCheckId = employmentCheckModel.Id;
                            employmentCheckCacheRequest.CorrelationId = employmentCheckModel.CorrelationId;
                            employmentCheckCacheRequest.RequestTypeId = (byte)RequestTypeEnum.EmploymentCheck;
                            employmentCheckCacheRequest.CreatedOn = DateTime.Now;
                            employmentCheckCacheRequest.LastUpdated = employmentCheckCacheRequest.CreatedOn;

                            await _employmentCheckRepository.StoreEmploymentCheckRequest(employmentCheckCacheRequest);

                            employmentCheckCacheRequests.Add(employmentCheckCacheRequest);
                        }
                        else
                        {
                            foreach (var error in validationResult.Errors)
                            {
                                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} EmploymentCheckMode {error.ErrorMessage}");
                            }
                        }
                    }

                    //// Store the requests
                    //var dbConnection = new DbConnection(); // TODO: move to startup DI
                    //if (dbConnection != null)
                    //{
                    //    await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                    //       _logger,
                    //       _connectionString,
                    //       AzureResource,
                    //       _azureServiceTokenProvider))
                    //    {
                    //        if (sqlConnection != null)
                    //        {
                    //            var repository = new DbRepository(_logger, _connectionString, AzureResource);
                    //            if (repository != null)
                    //            {
                    //                foreach (var employmentCheckCacheRequest in employmentCheckCacheRequests)
                    //                {
                    //                    var result = await repository.Insert(employmentCheckCacheRequest);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(employmentCheckCacheRequests);
        }

        private bool IsValid(EmploymentCheckModel employmentCheckModel)
        {
            var thisMethodName = $"{ThisClassName}.IsValid()";


            bool result = false;

            if (employmentCheckModel.Id > 0)
            {
                result = true;
            }
            else
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckModel Id input parameter is invalid {employmentCheckModel.Id}.");
            }

            return result;
        }

        /// <summary>
        /// Adds an employment check message to the HMRC API message queue
        /// </summary>
        /// <param name="employmentCheckData"></param>
        /// <returns>Task</returns>
        public async Task EnqueueEmploymentCheckMessages(EmploymentCheckData employmentCheckData)
        {
            // TODO: Add implementation for using Azure SqlDatabase
            var thisMethodName = $"{ThisClassName}.EnqueueEmploymentCheckMessages()";

            try
            {
                if (employmentCheckData != null)
                {
                    var employmentCheckMessages = await CreateEmploymentCheckMessages(_logger, employmentCheckData);

                    if (employmentCheckMessages != null && employmentCheckMessages.Any())
                    {
                        await StoreEmploymentCheckMessages(
                            _logger,
                            _connectionString,
                            AzureResource,
                            _azureServiceTokenProvider,
                            employmentCheckMessages);
                    }
                    else
                    {
                        _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessages value returned from StoreEmploymentCheckMessages() is null.");
                    }
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

        public async Task<IList<EmploymentCheckMessage>> CreateEmploymentCheckMessages(
            ILogger logger,
            EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"EmploymentCheckService.CreateEmploymentCheckMessages()";

            var employmentCheckMessages = new List<EmploymentCheckMessage>();
            try
            {
                if (employmentCheckData != null &&
                    employmentCheckData.EmploymentChecks != null &&
                    employmentCheckData.ApprenticeNiNumbers != null &&
                    employmentCheckData.EmployerPayeSchemes != null)
                {
                    // Create an EmploymentCheckMessage for each combination of Uln, National Insurance Number and PayeSchemes.
                    // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln and National Insurance Number
                    // Note: Once we get a 'match' for a given Nino, PayeScheme, MinDate, MaxDate from the HMRC API we could discard the remaining messages without calling the API if necessary
                    // (at this point we don't know if we need to check all the payeschemes for a given apprentice just to ensure there's no match on multiple schemes)
                    foreach (var employmentCheck in employmentCheckData.EmploymentChecks)
                    {
                        // Get the National Insurance Number for this apprentice
                        var nationalInsuranceNumber = await LookupNationalInsuranceNumberInEmploymentData(logger, employmentCheck.Uln, employmentCheckData.ApprenticeNiNumbers);
                        if (string.IsNullOrEmpty(nationalInsuranceNumber))
                        {
                            // No national insurance number found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                            logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} No national insurance number found for apprentice Uln: [{employmentCheck.Uln}].");
                            continue;
                        }

                        // Get the employer paye schemes for this apprentices
                        var employerPayeSchemes = await LookupPayeSchemeInEmploymentData(logger, employmentCheck.AccountId, employmentCheckData.EmployerPayeSchemes);
                        if (employerPayeSchemes == null)
                        {
                            // No paye schems found for this apprentice so we're not able to do an employment and will need to skip this apprentice
                            logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} No PAYE schemes found for apprentice Uln: [{employmentCheck.Uln}] accountId [{employmentCheck.AccountId}].");
                            continue;
                        }

                        // Create the individual message combinations for each paye scheme
                        foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                        {
                            if (employmentCheck.Id > 0 &&
                                employmentCheck.Uln > 0 &&
                                !string.IsNullOrEmpty(nationalInsuranceNumber) &&
                                employmentCheck.MinDate > DateTime.MinValue &&
                                employmentCheck.MaxDate > DateTime.MinValue &&
                                !string.IsNullOrEmpty(payeScheme))
                            {
                                var employmentCheckMessage = new EmploymentCheckMessage();

                                employmentCheckMessage.EmploymentCheckId = employmentCheck.Id;
                                employmentCheckMessage.Uln = employmentCheck.Uln;
                                employmentCheckMessage.NationalInsuranceNumber = nationalInsuranceNumber;
                                employmentCheckMessage.MinDateTime = employmentCheck.MinDate;
                                employmentCheckMessage.MaxDateTime = employmentCheck.MaxDate;
                                employmentCheckMessage.PayeScheme = payeScheme;

                                employmentCheckMessages.Add(employmentCheckMessage);
                            }
                            else
                            {
                                // There was invalid data preventing a message being created
                                logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} One of the following input values for creating an EmploymentCheckMessage was invalid: \n" +
                                    $"employmentCheck.Id: [{employmentCheck.Id}] should be > 0, \n" +
                                    $"employmentCheck.Uln: [{employmentCheck.Uln}] should be > 0, \n" +
                                    $"nationalInsuranceNumber: [{nationalInsuranceNumber}] should be not null and not empty, \n" +
                                    $"employmentCheck.MinDate: [{employmentCheck.MinDate}] should be > {DateTime.MinValue}, \n" +
                                    $"employmentCheck.MaxDate: [{employmentCheck.MaxDate}] should be > {DateTime.MinValue}, \n" +
                                    $"payeScheme: [{payeScheme}] should be not null and not empty.\n");
                            }
                        }
                    }
                }
                else
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The Employment Check Data supplied to create Employment Check Messages is incomplete.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(employmentCheckMessages);
        }

        private async Task<string> LookupNationalInsuranceNumberInEmploymentData(
            ILogger logger,
            long uln,
            IList<ApprenticeNiNumber> apprenticeNiNumbers)
        {
            string nationalInsuranceNumber = string.Empty;

            try
            {
                nationalInsuranceNumber = apprenticeNiNumbers.Where(ninumber => ninumber.Uln == uln).FirstOrDefault().NationalInsuranceNumber;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(nationalInsuranceNumber);
        }

        private async Task<EmployerPayeSchemes> LookupPayeSchemeInEmploymentData(
            ILogger logger,
            long accountId,
            IList<EmployerPayeSchemes> employerPayeSchemesList)
        {
            EmployerPayeSchemes employerPayeSchemes = null;

            try
            {
                employerPayeSchemes = employerPayeSchemesList.Where(ps => ps.EmployerAccountId == accountId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(employerPayeSchemes);
        }


        /// <summary>
        /// Creates the individual employment check messages and stores them in the database table queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="EmploymentCheckMessages"></param>
        /// <returns>Task</returns>
        private async Task StoreEmploymentCheckMessages(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            IList<EmploymentCheckMessage> employmentCheckMessages)
        {
            var thisMethodName = $"{ThisClassName}.StoreEmploymentCheckMessages()";

            try
            {
                if (employmentCheckMessages != null &&
                    employmentCheckMessages.Any())
                {
                    var dbConnection = new DbConnection(); // TODO: move to startup DI
                    if (dbConnection != null)
                    {
                        await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                            logger,
                            connectionString,
                            azureResource,
                            azureServiceTokenProvider))
                        {
                            if (sqlConnection != null)
                            {
                                await sqlConnection.OpenAsync();

                                foreach (var employmentCheckMessage in employmentCheckMessages)
                                {
                                    await _employmentCheckRepository.StoreEmploymentCheckMessage(employmentCheckMessage);

                                    //await StoreEmploymentCheckMessage(
                                    //    logger,
                                    //    connectionString,
                                    //    sqlConnection,
                                    //    azureResource,
                                    //    azureServiceTokenProvider,
                                    //    employmentCheckMessage);
                                }
                            }
                            else
                            {
                                logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Creation of SQL Connection for the Employment Check Database failed.");
                            }
                        }
                    }
                }
                else
                {
                    logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No employment check queue messaages to store.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        ///// <summary>
        ///// Store an individual employment check message in the database table queue
        ///// </summary>
        ///// <param name="logger"></param>
        ///// <param name="connectionString"></param>
        ///// <param name="azureResource"></param>
        ///// <param name="azureServiceTokenProvider"></param>
        ///// <param name="employmentCheckMessage"></param>
        ///// <returns>Task</returns>
        //public async Task StoreEmploymentCheckMessage(
        //    ILogger logger,
        //    string connectionString,
        //    SqlConnection sqlConnection,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider,
        //    EmploymentCheckMessage employmentCheckMessage)
        //{
        //    var thisMethodName = $"{ThisClassName}.StoreEmploymentCheckMessage()";

        //    try
        //    {
        //        if (employmentCheckMessage != null)
        //        {
        //            if (sqlConnection != null)
        //            {
        //                var transaction = sqlConnection.BeginTransaction();
        //                {
        //                    try
        //                    {
        //                        var parameter = new DynamicParameters();
        //                        parameter.Add("@employmentCheckId", employmentCheckMessage.EmploymentCheckId, DbType.Int64);
        //                        parameter.Add("@correlationId", employmentCheckMessage.CorrelationId, DbType.Guid);
        //                        parameter.Add("@uln", employmentCheckMessage.Uln, DbType.Int64);
        //                        parameter.Add("@nationalInsuranceNumber", employmentCheckMessage.NationalInsuranceNumber, DbType.String);
        //                        parameter.Add("@payeScheme", employmentCheckMessage.PayeScheme, DbType.String);
        //                        parameter.Add("@minDateTime", employmentCheckMessage.MinDateTime, DbType.DateTime);
        //                        parameter.Add("@maxDateTime", employmentCheckMessage.MaxDateTime, DbType.DateTime);
        //                        parameter.Add("@employed", employmentCheckMessage.Employed ?? false, DbType.Boolean);
        //                        parameter.Add("@lastEmploymentCheck", employmentCheckMessage.LastEmploymentCheck, DbType.DateTime);
        //                        parameter.Add("@responseHttpStatusCode", employmentCheckMessage.ResponseHttpStatusCode, DbType.Int16);
        //                        parameter.Add("@responseMessage", employmentCheckMessage.ResponseMessage, DbType.String);
        //                        parameter.Add("@createdOn", employmentCheckMessage.CreatedOn = DateTime.Now, DbType.DateTime);

        //                        await sqlConnection.ExecuteAsync(
        //                            "INSERT [SFA.DAS.EmploymentCheck.Database].[Cache].[EmploymentCheckMessageQueue] " +
        //                            "       ( EmploymentCheckId,  CorrelationId, Uln,   NationalInsuranceNumber,  PayeScheme,  MinDateTime,  MaxDateTime,  Employed,  LastEmploymentCheck,  ResponseHttpStatusCode,  ResponseMessage,  CreatedOn) " +
        //                            "VALUES (@employmentCheckId, @correlationId, @uln, @nationalInsuranceNumber, @payeScheme, @minDateTime, @maxDateTime, @employed, @lastEmploymentCheck, @ResponseHttpStatusCode, @ResponseMessage, @createdOn)",
        //                            parameter,
        //                            commandType: CommandType.Text,
        //                            transaction: transaction);

        //                        transaction.Commit();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        transaction.Rollback();
        //                        logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No employment check queue messaages to store.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }
        //}

        /// <summary>
        /// Gets an employment check message from the HMRC API message queue
        /// </summary>
        /// <returns>Task<EmploymentCheckMessage></returns>
        public async Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage()
        {
            var thisMethodName = $"{ThisClassName}.DequeueEmploymentCheckMessage()";

            EmploymentCheckMessage employmentCheckMessage = null;
            try
            {
                employmentCheckMessage = await GetEmploymentCheckMessage(
                    _logger,
                    _connectionString,
                    AzureResource,
                    _batchSize,
                    _azureServiceTokenProvider);

                if (employmentCheckMessage == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessage value returned from the call to GetEmploymentCheckMessage_Base() is null.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckMessage;
        }

        /// <summary>
        /// Gets a single message from the db table
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns></returns>
        public async Task<EmploymentCheckMessage> GetEmploymentCheckMessage(
              ILogger logger,
              string connectionString,
              string azureResource,
              int batchSize,
              AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckMessage_Base()";

            EmploymentCheckMessage employmentCheckMessage = null;
            SqlConnection sqlConnection = null;

            try
            {
                var dbConnection = new DbConnection(); // TODO: Move to startup DI
                if (dbConnection != null)
                {
                    await using (sqlConnection = await dbConnection.CreateSqlConnection(
                    logger,
                    connectionString,
                    azureResource,
                    azureServiceTokenProvider))
                    {
                        if (sqlConnection != null)
                        {
                            await sqlConnection.OpenAsync();

                            // TODO: The expectation is that there is only one instance of this code getting the message from the database.
                            //       If there are going to be multiple instances of the code pulling the messages off the message queue
                            //       then we will need to make the select/delete of the message transactional to lock the row so that
                            //       other instances aren't pulling the same message

                            const string sql = "SELECT TOP (1) " +
                                               "Id, " +
                                               "EmploymentCheckId, " +
                                               "Uln, " +
                                               "NationalInsuranceNumber, " +
                                               "PayeScheme, " +
                                               "MinDateTime, " +
                                               "MaxDateTime, " +
                                               "Employed, " +
                                               "LastEmploymentCheck, " +
                                               "ResponseHttpStatusCode, " +
                                               "ResponseMessage " +
                                               "FROM [Cache].[EmploymentCheckMessageQueue] " +
                                               "ORDER BY Id";

                            employmentCheckMessage = (await sqlConnection.QueryAsync<EmploymentCheckMessage>(sql, commandType: CommandType.Text)).FirstOrDefault();

                            if (employmentCheckMessage == null)
                            {
                                logger.LogInformation(
                                    $"{thisMethodName}: {ErrorMessagePrefix} The EmploymentCheckMessage returned from the LINQ statement employmentCheckMessage.FirstOrDefault() is null.");
                            }
                        }
                        else
                        {
                            logger.LogInformation(
                                $"{thisMethodName}: {ErrorMessagePrefix} The sqlConnection value returned from CreateSqlConnection() is null.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"{thisMethodName} {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckMessage;
        }


        public async Task SaveEmploymentCheckResult(
            EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckResult_Service()";

            try
            {
                if (employmentCheckMessage != null)
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

                                var transaction = sqlConnection.BeginTransaction();
                                {
                                    try
                                    {
                                        // -------------------------------------------------------------------
                                        // Store the employment status
                                        // -------------------------------------------------------------------
                                        var employmentCheckParameters = new DynamicParameters();
                                        employmentCheckParameters.Add("id", employmentCheckMessage.EmploymentCheckId, DbType.Int64);
                                        employmentCheckParameters.Add("messageId", employmentCheckMessage.Id, DbType.Int64);
                                        employmentCheckParameters.Add("employed", employmentCheckMessage.Employed, DbType.Boolean);

                                        await sqlConnection.ExecuteAsync(
                                            "UPDATE [Business].[EmploymentCheck] " +
                                            "SET Employed = @employed, " +
                                            "LastUpdated = GETDATE() " +
                                            "WHERE Id = @id",
                                                employmentCheckParameters,
                                                commandType: CommandType.Text,
                                                transaction: transaction);

                                        // -------------------------------------------------------------------
                                        // Archive a copy of the employment check message for auditing
                                        // -------------------------------------------------------------------
                                        var employmentCheckMessageHistoryModel = new EmploymentCheckMessageHistory(employmentCheckMessage);

                                        var messageHistoryParameters = new DynamicParameters();
                                        messageHistoryParameters.Add("@messageId", employmentCheckMessageHistoryModel.MessageId, DbType.Int64);
                                        messageHistoryParameters.Add("@employmentCheckId", employmentCheckMessageHistoryModel.EmploymentCheckId, DbType.Int64);
                                        messageHistoryParameters.Add("@correlationId", employmentCheckMessageHistoryModel.CorrelationId, DbType.Guid);
                                        messageHistoryParameters.Add("@uln", employmentCheckMessageHistoryModel.Uln, DbType.Int64);
                                        messageHistoryParameters.Add("@nationalInsuranceNumber", employmentCheckMessageHistoryModel.NationalInsuranceNumber, DbType.String);
                                        messageHistoryParameters.Add("@payeScheme", employmentCheckMessageHistoryModel.PayeScheme, DbType.String);
                                        messageHistoryParameters.Add("@minDateTime", employmentCheckMessageHistoryModel.MinDateTime, DbType.DateTime);
                                        messageHistoryParameters.Add("@maxDateTime", employmentCheckMessageHistoryModel.MaxDateTime, DbType.DateTime);
                                        messageHistoryParameters.Add("@employed", employmentCheckMessageHistoryModel.Employed ?? false, DbType.Boolean);
                                        messageHistoryParameters.Add("@lastEmploymentCheck", employmentCheckMessageHistoryModel.LastEmploymentCheck, DbType.DateTime);
                                        messageHistoryParameters.Add("@responseHttpStatusCode", employmentCheckMessageHistoryModel.ResponseHttpStatusCode, DbType.Int16);
                                        messageHistoryParameters.Add("@responseMessage", employmentCheckMessageHistoryModel.ResponseMessage, DbType.String);
                                        messageHistoryParameters.Add("@messageCreatedOn", employmentCheckMessageHistoryModel.MessageCreatedOn = DateTime.Now, DbType.DateTime);
                                        messageHistoryParameters.Add("@createdOn", employmentCheckMessageHistoryModel.CreatedOn = DateTime.Now, DbType.DateTime);

                                        await sqlConnection.ExecuteAsync(
                                            "INSERT [SFA.DAS.EmploymentCheck.Database].[Cache].[EmploymentCheckMessageQueueHistory] " +
                                            "       ( [MessageId], [EmploymentCheckId], [CorrelationId], [Uln], [NationalInsuranceNumber], [PayeScheme], [MinDateTime], [MaxDateTime], [Employed], [LastEmploymentCheck], [ResponseHttpStatusCode], [ResponseMessage], [MessageCreatedOn], [CreatedOn]) " +
                                            "VALUES ( @messageId , @employmentCheckId , @correlationId , @uln , @nationalInsuranceNumber , @payeScheme , @minDateTime , @maxDateTime , @employed , @lastEmploymentCheck , @responseHttpStatusCode , @responseMessage , @messageCreatedOn , @createdOn)",
                                            messageHistoryParameters,
                                            commandType: CommandType.Text,
                                            transaction: transaction);

                                        // -------------------------------------------------------------------
                                        // Remove the employment check message from the queue
                                        // -------------------------------------------------------------------
                                        var messageParameters = new DynamicParameters();
                                        messageParameters.Add("id", employmentCheckMessage.Id, DbType.Int64);

                                        await sqlConnection.ExecuteAsync(
                                            "DELETE [Cache].[EmploymentCheckMessageQueue] WHERE Id = @id",
                                                messageParameters,
                                                commandType: CommandType.Text,
                                                transaction: transaction);

                                        transaction.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();
                                        _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessage input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

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
                                parameters.Add("@minDate", input.MinDateTime, DbType.DateTime);
                                parameters.Add("@maxDate", input.MaxDateTime, DbType.DateTime);
                                parameters.Add("@employed", false, DbType.Boolean);
                                parameters.Add("@lastUpdated", now, DbType.DateTime);
                                parameters.Add("@createdOn", now, DbType.DateTime);


                                await sqlConnection.ExecuteAsync(
                                    "INSERT [Business].[EmploymentCheck] (" +
                                    "CorrelationId, " +
                                    "CheckType, " +
                                    "Uln, " +
                                    "ApprenticeshipId, " +
                                    "AccountId, " +
                                    "MinDate, " +
                                    "MaxDate, " +
                                    "Employed, " +
                                    "LastUpdated, " +
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
                                    "@lastUpdated, " +
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

        //private async Task<long> GetEmploymentCheckLastHighestBatchId(
        //    ILogger logger,
        //    SqlConnection sqlConnection,
        //    SqlTransaction transaction)
        //{
        //    var thisMethodName = $"{ThisClassName}.GetEmploymentCheckLastHighestBatchId()";

        //    long employmentCheckLastHighestBatchId = 0;
        //    try
        //    {
        //        // Get the highest EmploymentCheckId from the message queue
        //        // if there are no messages in the message queue
        //        // Get the highest EmploymentCheckId from the message queue history
        //        // if there are no messages in the message queue history
        //        // use the default value of zero
        //        logger.LogInformation($"{thisMethodName}: Getting the EmploymentCheckLastHighestBatchId");
        //        employmentCheckLastHighestBatchId =
        //        (
        //            await sqlConnection.QueryAsync<long>(
        //                sql: "SELECT ISNULL(MAX(EmploymentCheckId), 0) FROM [Cache].[EmploymentCheckMessageQueue] WITH (NOLOCK)",
        //                commandType: CommandType.Text, transaction: transaction)
        //        ).FirstOrDefault();

        //        if (employmentCheckLastHighestBatchId == 0)
        //        {
        //            employmentCheckLastHighestBatchId =
        //            (
        //                await sqlConnection.QueryAsync<long>(
        //                    sql: "SELECT ISNULL(MAX(EmploymentCheckId), 0) FROM [Cache].[EmploymentCheckMessageQueueHistory] WITH (NOLOCK)",
        //                    commandType: CommandType.Text, transaction: transaction)
        //            ).FirstOrDefault();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        transaction.Rollback();
        //        logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the employmentCheckLastHighestBatchId failed - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return employmentCheckLastHighestBatchId;
        //}

        //private async Task<SqlConnection> CreateSqlConnection(
        //    ILogger logger,
        //    string connectionString,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider)
        //{
        //    var thisMethodName = $"{ThisClassName}.CreateSqlConnection()";

        //    SqlConnection sqlConnection = null;
        //    try
        //    {
        //        if (!String.IsNullOrEmpty(connectionString))
        //        {
        //            if (!String.IsNullOrEmpty(azureResource))
        //            {
        //                sqlConnection = new SqlConnection(connectionString);
        //                if (sqlConnection != null)
        //                {
        //                    // no token for local db
        //                    if (azureServiceTokenProvider != null)
        //                    {
        //                        sqlConnection.AccessToken = await azureServiceTokenProvider.GetAccessTokenAsync(azureResource);
        //                    }
        //                    else
        //                    {
        //                        logger.LogInformation($"{thisMethodName}: azureServiceTokenProvider was not specified, call to azureServiceTokenProvider.GetAccessTokenAsync(azureResource) skipped.");
        //                    }
        //                }
        //                else
        //                {
        //                    logger.LogInformation($"{thisMethodName}: Missing AzureResource string for the Employment Check Databasse.");
        //                }
        //            }
        //            else
        //            {
        //                logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Missing SQL connecton string for the Employment Check Databasse.");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return sqlConnection;
        //}

       public static EmploymentCheckMessage[] StubEmploymentCheckMessageData => new[] {
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
            }
       };
    }
}