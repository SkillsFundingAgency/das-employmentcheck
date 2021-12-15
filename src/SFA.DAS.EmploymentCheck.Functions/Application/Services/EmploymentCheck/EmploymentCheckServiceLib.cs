using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    //public abstract class EmploymentCheckServiceLib
    //{
    //    private const string ThisClassName = "\n\nEmploymentCheckServiceBase";
    //    public const string ErrorMessagePrefix = "[*** ERROR ***]";

        // The EmploymentCheckService and EmploymentCheckServiceStub can implement this method to either call the database code or the stub code as appropriate
        //public abstract Task<IList<EmploymentCheckModel>> GetEmploymentChecksBatch_Service(long employmentCheckLastHighestBatchId);

        // The EmploymentCheckService and EmploymentCheckServiceStub can implement this method to either call the database code or the stub code as appropriate
        //public abstract Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests_Service(long employmentCheckLastHighestBatchId);

        /// <summary>
        /// Gets a batch of the employment checks from the Employment Check database
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        //public virtual async Task<IList<EmploymentCheckModel>> GetEmploymentChecks_Base(
        //    ILogger logger,
        //    string connectionString,
        //    string azureResource,
        //    int batchSize,
        //    long employmentCheckLastHighestBatchId,  // TODO: Review if this is needed, current solution is to use a db control table to store the Id of the last record read when populating the message queue
        //    AzureServiceTokenProvider azureServiceTokenProvider)
        //{
        //    var thisMethodName = $"{ThisClassName}.GetEmploymentChecks_Base()";

        //    IList<Models.Domain.EmploymentCheckModel> EmploymentCheckModels = null;
        //    try
        //    {
        //        await using (var sqlConnection = await CreateSqlConnection(
        //           logger,
        //           connectionString,
        //           azureResource,
        //           azureServiceTokenProvider))
        //        {
        //            if (sqlConnection != null)
        //            {
        //                await sqlConnection.OpenAsync();

        //                // Get the Id of the last item in the previous batch
        //                employmentCheckLastHighestBatchId = await GetEmploymentCheckLastHighestBatchId(logger, sqlConnection, null);

        //                // Get a batch of records starting from the last highest id
        //                EmploymentCheckModels = await GetEmploymentCheckBatch(logger, sqlConnection, batchSize, employmentCheckLastHighestBatchId);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return EmploymentCheckModels;
        //}

        /// <summary>
        /// Creates an EmploymentCheckCacheRequest for each employment check in the given list of employment checks
        /// </summary>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        //public virtual async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
        //    ILogger logger,
        //    string connectionString,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider,
        //    IList<EmploymentCheckModel> employmentCheckModels)
        //{
        //    var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckCacheRequests()";

        //    IList<EmploymentCheckCacheRequest> employmentCheckCacheRequests = null;
        //    try
        //    {
        //        // TODO: Implmentation
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return await Task.FromResult(employmentCheckCacheRequests);
        //}

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

        //        if(employmentCheckLastHighestBatchId == 0)
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



        /// <summary>
        /// Adds an employment check message to the HMRC API message queue
        /// Note: This abstract class makes it a requirement to implement this method in the Service and the ServiceStub and for local testing the ServiceStub will
        /// call the implementation defined method below this one
        /// </summary>
        /// <param name="EmploymentCheckData"></param>
        /// <returns></returns>
        //public abstract Task EnqueueEmploymentCheckMessages_Service(EmploymentCheckData employmentCheckData);

        ///// <summary>
        ///// Adds an employment check message to the HMRC API message queue
        ///// </summary>
        ///// <param name="logger"></param>
        ///// <param name="connectionString"></param>
        ///// <param name="azureResource"></param>
        ///// <param name="azureServiceTokenProvider"></param>
        ///// <param name="EmploymentCheckData"></param>
        ///// <returns>Task</returns>
        //public virtual async Task EnqueueEmploymentCheckMessages_Service(
        //    ILogger logger,
        //    string connectionString,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider,
        //    EmploymentCheckData employmentCheckData)
        //{
        //    var thisMethodName = $"{ThisClassName}.EnqueueEmploymentCheckMessages_Service()";

        //    try
        //    {
        //        if (employmentCheckData != null)
        //        {
        //            var employmentCheckMessages = await CreateEmploymentCheckMessages(logger, employmentCheckData);

        //            if(employmentCheckMessages != null && employmentCheckMessages.Any())
        //            {
        //                await StoreEmploymentCheckMessages(
        //                    logger,
        //                    connectionString,
        //                    azureResource,
        //                    azureServiceTokenProvider,
        //                    employmentCheckMessages);
        //            }
        //            else
        //            {
        //                logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessages value returned from StoreEmploymentCheckMessages() is null.");
        //            }
        //        }
        //        else
        //        {
        //            logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckData input parameter is null.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }
        //}

        /// <summary>
        /// Gets an employment check message from the HMRC API message queue
        /// Note: This abstract class makes it a requirement to implement this method in the Service and the ServiceStub and for local testing the ServiceStub will
        /// call the implementation defined method below this one
        /// </summary>
        /// <returns></returns>
        //public abstract Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage_Service();

        /// <summary>
        /// Gets an employment check message from the HMRC API message queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<EmploymentCheckMessage></returns>
        //public virtual async Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage_Base(
        //    ILogger logger,
        //    string connectionString,
        //    int batchSize,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider)
        //{
        //    var thisMethodName = $"{ThisClassName}.DequeueEmploymentCheckMessage_Base()";

        //    EmploymentCheckMessage employmentCheckMessage = null;
        //    try
        //    {
        //        employmentCheckMessage = await GetEmploymentCheckMessage_Base(
        //            logger,
        //            connectionString,
        //            azureResource,
        //            batchSize,
        //            azureServiceTokenProvider);

        //        if (employmentCheckMessage == null)
        //        {
        //            logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessage value returned from the call to GetEmploymentCheckMessage_Base() is null.");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return employmentCheckMessage;
        //}

        /// <summary>
        /// Gets a single message from the db table
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns></returns>
        //public virtual async Task<EmploymentCheckMessage> GetEmploymentCheckMessage_Base(
        //      ILogger logger,
        //      string connectionString,
        //      string azureResource,
        //      int batchSize,
        //      AzureServiceTokenProvider azureServiceTokenProvider)
        //{
        //    var thisMethodName = $"{ThisClassName}.GetEmploymentCheckMessage_Base()";

        //    EmploymentCheckMessage employmentCheckMessage = null;
        //    SqlConnection sqlConnection = null;

        //    try
        //    {
        //        await using (sqlConnection = await CreateSqlConnection(
        //            logger,
        //            connectionString,
        //            azureResource,
        //            azureServiceTokenProvider))
        //        {
        //            if (sqlConnection != null)
        //            {
        //                await sqlConnection.OpenAsync();

        //                // TODO: The expectation is that there is only one instance of this code getting the message from the database.
        //                //       If there are going to be multiple instances of the code pulling the messages off the message queue
        //                //       then we will need to make the select/delete of the message transactional to lock the row so that
        //                //       other instances aren't pulling the same message

        //                const string sql = "SELECT TOP (1) " +
        //                                   "Id, " +
        //                                   "EmploymentCheckId, " +
        //                                   "Uln, " +
        //                                   "NationalInsuranceNumber, " +
        //                                   "PayeScheme, " +
        //                                   "MinDateTime, " +
        //                                   "MaxDateTime, " +
        //                                   "Employed, " +
        //                                   "LastEmploymentCheck, " +
        //                                   "ResponseHttpStatusCode, " +
        //                                   "ResponseMessage " +
        //                                   "FROM [Cache].[EmploymentCheckMessageQueue] " +
        //                                   "ORDER BY Id";

        //                employmentCheckMessage = (await sqlConnection.QueryAsync<EmploymentCheckMessage>(sql, commandType: CommandType.Text)).FirstOrDefault();

        //                if (employmentCheckMessage == null)
        //                {
        //                    logger.LogInformation(
        //                        $"{thisMethodName}: {ErrorMessagePrefix} The EmploymentCheckMessage returned from the LINQ statement employmentCheckMessage.FirstOrDefault() is null.");
        //                }
        //            }
        //            else
        //            {
        //                logger.LogInformation(
        //                    $"{thisMethodName}: {ErrorMessagePrefix} The sqlConnection value returned from CreateSqlConnection() is null.");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(
        //            $"{thisMethodName} {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return employmentCheckMessage;
        //}

        //public abstract Task SaveEmploymentCheckResult_Service(EmploymentCheckMessage employmentCheckMessage);

        //public virtual async Task SaveEmploymentCheckResult_Base(
        //    ILogger logger,
        //    string connectionString,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider,
        //    EmploymentCheckMessage employmentCheckMessage)
        //{
        //    var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckResult_Base()";

        //    try
        //    {
        //        if (employmentCheckMessage != null)
        //        {
        //            var sqlConnection = await CreateSqlConnection(logger, connectionString, azureResource, azureServiceTokenProvider);
        //            if (sqlConnection != null)
        //            {
        //                await sqlConnection.OpenAsync();

        //                var transaction = sqlConnection.BeginTransaction();
        //                {
        //                    try
        //                    {
        //                        // -------------------------------------------------------------------
        //                        // Store the employment status
        //                        // -------------------------------------------------------------------
        //                        var employmentCheckParameters = new DynamicParameters();
        //                        employmentCheckParameters.Add("id", employmentCheckMessage.EmploymentCheckId, DbType.Int64);
        //                        employmentCheckParameters.Add("messageId", employmentCheckMessage.Id, DbType.Int64);
        //                        employmentCheckParameters.Add("employed", employmentCheckMessage.Employed, DbType.Boolean);

        //                        await sqlConnection.ExecuteAsync(
        //                            "UPDATE [Business].[EmploymentCheck] " +
        //                            "SET Employed = @employed, " +
        //                            "LastUpdated = GETDATE() " +
        //                            "WHERE Id = @id",
        //                                employmentCheckParameters,
        //                                commandType: CommandType.Text,
        //                                transaction: transaction);

        //                        // -------------------------------------------------------------------
        //                        // Archive a copy of the employment check message for auditing
        //                        // -------------------------------------------------------------------
        //                        var employmentCheckMessageHistoryModel = new EmploymentCheckMessageHistory(employmentCheckMessage);

        //                        var messageHistoryParameters = new DynamicParameters();
        //                        messageHistoryParameters.Add("@messageId", employmentCheckMessageHistoryModel.MessageId, DbType.Int64);
        //                        messageHistoryParameters.Add("@employmentCheckId", employmentCheckMessageHistoryModel.EmploymentCheckId, DbType.Int64);
        //                        messageHistoryParameters.Add("@correlationId", employmentCheckMessageHistoryModel.CorrelationId, DbType.Guid);
        //                        messageHistoryParameters.Add("@uln", employmentCheckMessageHistoryModel.Uln, DbType.Int64);
        //                        messageHistoryParameters.Add("@nationalInsuranceNumber", employmentCheckMessageHistoryModel.NationalInsuranceNumber, DbType.String);
        //                        messageHistoryParameters.Add("@payeScheme", employmentCheckMessageHistoryModel.PayeScheme, DbType.String);
        //                        messageHistoryParameters.Add("@minDateTime", employmentCheckMessageHistoryModel.MinDateTime, DbType.DateTime);
        //                        messageHistoryParameters.Add("@maxDateTime", employmentCheckMessageHistoryModel.MaxDateTime, DbType.DateTime);
        //                        messageHistoryParameters.Add("@employed", employmentCheckMessageHistoryModel.Employed ?? false, DbType.Boolean);
        //                        messageHistoryParameters.Add("@lastEmploymentCheck", employmentCheckMessageHistoryModel.LastEmploymentCheck, DbType.DateTime);
        //                        messageHistoryParameters.Add("@responseHttpStatusCode", employmentCheckMessageHistoryModel.ResponseHttpStatusCode, DbType.Int16);
        //                        messageHistoryParameters.Add("@responseMessage", employmentCheckMessageHistoryModel.ResponseMessage, DbType.String);
        //                        messageHistoryParameters.Add("@messageCreatedOn", employmentCheckMessageHistoryModel.MessageCreatedOn = DateTime.Now, DbType.DateTime);
        //                        messageHistoryParameters.Add("@createdOn", employmentCheckMessageHistoryModel.CreatedOn = DateTime.Now, DbType.DateTime);

        //                        await sqlConnection.ExecuteAsync(
        //                            "INSERT [SFA.DAS.EmploymentCheck.Database].[Cache].[EmploymentCheckMessageQueueHistory] " +
        //                            "       ( [MessageId], [EmploymentCheckId], [CorrelationId], [Uln], [NationalInsuranceNumber], [PayeScheme], [MinDateTime], [MaxDateTime], [Employed], [LastEmploymentCheck], [ResponseHttpStatusCode], [ResponseMessage], [MessageCreatedOn], [CreatedOn]) " +
        //                            "VALUES ( @messageId , @employmentCheckId , @correlationId , @uln , @nationalInsuranceNumber , @payeScheme , @minDateTime , @maxDateTime , @employed , @lastEmploymentCheck , @responseHttpStatusCode , @responseMessage , @messageCreatedOn , @createdOn)",
        //                            messageHistoryParameters,
        //                            commandType: CommandType.Text,
        //                            transaction: transaction);

        //                        // -------------------------------------------------------------------
        //                        // Remove the employment check message from the queue
        //                        // -------------------------------------------------------------------
        //                        var messageParameters = new DynamicParameters();
        //                        messageParameters.Add("id", employmentCheckMessage.Id, DbType.Int64);

        //                        await sqlConnection.ExecuteAsync(
        //                            "DELETE [Cache].[EmploymentCheckMessageQueue] WHERE Id = @id",
        //                                messageParameters,
        //                                commandType: CommandType.Text,
        //                                transaction: transaction);

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
        //            logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessage input parameter is null.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }
        //}


        //private async Task<string> LookupNationalInsuranceNumberInEmploymentData(
        //    ILogger logger,
        //    long uln,
        //    IList<ApprenticeNiNumber> apprenticeNiNumbers)
        //{
        //    string nationalInsuranceNumber = string.Empty;

        //    try
        //    {
        //        nationalInsuranceNumber = apprenticeNiNumbers.Where(ninumber => ninumber.Uln == uln).FirstOrDefault().NationalInsuranceNumber;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return await Task.FromResult(nationalInsuranceNumber);
        //}

        //private async Task<EmployerPayeSchemes> LookupPayeSchemeInEmploymentData(
        //    ILogger logger,
        //    long accountId,
        //    IList<EmployerPayeSchemes> employerPayeSchemesList)
        //{
        //    EmployerPayeSchemes employerPayeSchemes = null;

        //    try
        //    {
        //         employerPayeSchemes = employerPayeSchemesList.Where(ps => ps.EmployerAccountId == accountId).FirstOrDefault();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return await Task.FromResult(employerPayeSchemes);
        //}

        /// <summary>
        /// Creates the individual employment check messages and stores them in the database table queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="EmploymentCheckMessages"></param>
        /// <returns>Task</returns>
        //public virtual async Task StoreEmploymentCheckMessages(
        //    ILogger logger,
        //    string connectionString,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider,
        //    IList<EmploymentCheckMessage> employmentCheckMessages)
        //{
        //    var thisMethodName = $"{ThisClassName}.StoreEmploymentCheckMessages()";

        //    try
        //    {
        //        if (employmentCheckMessages != null &&
        //            employmentCheckMessages.Any())
        //        {
        //            await using (var sqlConnection = await CreateSqlConnection(
        //                logger,
        //                connectionString,
        //                azureResource,
        //                azureServiceTokenProvider))
        //            {
        //                if (sqlConnection != null)
        //                {
        //                    await sqlConnection.OpenAsync();

        //                    foreach (var employmentCheckMessage in employmentCheckMessages)
        //                    {
        //                        await StoreEmploymentCheckMessage(
        //                            logger,
        //                            connectionString,
        //                            sqlConnection,
        //                            azureResource,
        //                            azureServiceTokenProvider,
        //                            employmentCheckMessage);
        //                    }
        //                }
        //                else
        //                {
        //                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Creation of SQL Connection for the Employment Check Database failed.");
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
        /// Store an individual employment check message in the database table queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="employmentCheckMessage"></param>
        /// <returns>Task</returns>
        //public virtual async Task StoreEmploymentCheckMessage(
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
        //            if (sqlConnection == null)
        //            {
        //                sqlConnection = await CreateSqlConnection(
        //                    logger,
        //                    connectionString,
        //                    azureResource,
        //                    azureServiceTokenProvider);

        //                await sqlConnection.OpenAsync();
        //            }

        //            var transaction = sqlConnection.BeginTransaction();
        //            {
        //                try
        //                {
        //                    var parameter = new DynamicParameters();
        //                    parameter.Add("@employmentCheckId", employmentCheckMessage.EmploymentCheckId, DbType.Int64);
        //                    parameter.Add("@correlationId", employmentCheckMessage.CorrelationId, DbType.Guid);
        //                    parameter.Add("@uln", employmentCheckMessage.Uln, DbType.Int64);
        //                    parameter.Add("@nationalInsuranceNumber", employmentCheckMessage.NationalInsuranceNumber, DbType.String);
        //                    parameter.Add("@payeScheme", employmentCheckMessage.PayeScheme, DbType.String);
        //                    parameter.Add("@minDateTime", employmentCheckMessage.MinDateTime, DbType.DateTime);
        //                    parameter.Add("@maxDateTime", employmentCheckMessage.MaxDateTime, DbType.DateTime);
        //                    parameter.Add("@employed", employmentCheckMessage.Employed ?? false, DbType.Boolean);
        //                    parameter.Add("@lastEmploymentCheck", employmentCheckMessage.LastEmploymentCheck, DbType.DateTime);
        //                    parameter.Add("@responseHttpStatusCode", employmentCheckMessage.ResponseHttpStatusCode, DbType.Int16);
        //                    parameter.Add("@responseMessage", employmentCheckMessage.ResponseMessage, DbType.String);
        //                    parameter.Add("@createdOn", employmentCheckMessage.CreatedOn = DateTime.Now, DbType.DateTime);

        //                    await sqlConnection.ExecuteAsync(
        //                        "INSERT [SFA.DAS.EmploymentCheck.Database].[Cache].[EmploymentCheckMessageQueue] " +
        //                        "       ( EmploymentCheckId,  CorrelationId, Uln,   NationalInsuranceNumber,  PayeScheme,  MinDateTime,  MaxDateTime,  Employed,  LastEmploymentCheck,  ResponseHttpStatusCode,  ResponseMessage,  CreatedOn) " +
        //                        "VALUES (@employmentCheckId, @correlationId, @uln, @nationalInsuranceNumber, @payeScheme, @minDateTime, @maxDateTime, @employed, @lastEmploymentCheck, @ResponseHttpStatusCode, @ResponseMessage, @createdOn)",
        //                        parameter,
        //                        commandType: CommandType.Text,
        //                        transaction: transaction);

        //                    transaction.Commit();
        //                }
        //                catch (Exception ex)
        //                {
        //                    transaction.Rollback();
        //                    logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
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



        //public abstract Task SeedEmploymentCheckDatabaseTableTestData();


        //public virtual async Task SeedEmploymentCheckDatabaseTableTestData(
        //    ILogger logger,
        //    string connectionString,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider)
        //{
        //    var thisMethodName = $"{ThisClassName}.SeedEmploymentCheckDatabaseTableTestData()";

        //    try
        //    {
        //        await using (var connection = await CreateSqlConnection(
        //            logger,
        //            connectionString,
        //            azureResource,
        //            azureServiceTokenProvider))
        //        {
        //            if (connection != null)
        //            {
        //                await connection.OpenAsync();

        //                var i = 0;
        //                foreach (var input in EmploymentCheckServiceStub.StubEmploymentCheckMessageData)
        //                {
        //                    i++;
        //                    var now = DateTime.Now;
        //                    var parameters = new DynamicParameters();
        //                    parameters.Add("@correlationId", new Guid(), DbType.Guid);
        //                    parameters.Add("@checkType", "StartDate+60", DbType.String);
        //                    parameters.Add("@uln", 1000000000 + i, DbType.Int64);
        //                    parameters.Add("@apprenticeshipId", i, DbType.Int64);
        //                    parameters.Add("@accountId", i, DbType.Int64);
        //                    parameters.Add("@minDate", input.MinDateTime, DbType.DateTime);
        //                    parameters.Add("@maxDate", input.MaxDateTime, DbType.DateTime);
        //                    parameters.Add("@employed", false, DbType.Boolean);
        //                    parameters.Add("@lastUpdated", now, DbType.DateTime);
        //                    parameters.Add("@createdOn", now, DbType.DateTime);


        //                    await connection.ExecuteAsync(
        //                        "INSERT [Business].[EmploymentCheck] (" +
        //                        "CorrelationId, " +
        //                        "CheckType, " +
        //                        "Uln, " +
        //                        "ApprenticeshipId, " +
        //                        "AccountId, " +
        //                        "MinDate, " +
        //                        "MaxDate, " +
        //                        "Employed, " +
        //                        "LastUpdated, " +
        //                        "CreatedOn) " +
        //                        "VALUES (" +
        //                        "@correlationId, " +
        //                        "@checkType, " +
        //                        "@uln, " +
        //                        "@apprenticeshipId, " +
        //                        "@accountId, " +
        //                        "@minDate, " +
        //                        "@maxDate, " +
        //                        "@employed, " +
        //                        "@lastUpdated, " +
        //                        "@createdOn) ",
        //                        commandType: CommandType.Text,
        //                        param: parameters);
        //                }
        //            }
        //            else
        //            {
        //                logger.LogInformation(
        //                    $"\n\n{DateTime.UtcNow} {thisMethodName}: *** ERROR ***: Creation of SQL Connection for the Employment Check Database failed.");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }
        //}
 //   }
}