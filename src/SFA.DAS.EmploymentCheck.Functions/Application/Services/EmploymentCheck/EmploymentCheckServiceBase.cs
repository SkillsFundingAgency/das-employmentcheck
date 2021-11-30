﻿using System;
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
    public abstract class EmploymentCheckServiceBase
        : IEmploymentCheckService
    {
        private const string ThisClassName = "\n\nEmploymentCheckServiceBase";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        // The EmploymentCheckService and EmploymentCheckServiceStub can implement this method to either call the database code or the stub code as appropriate
        public abstract Task<IList<Models.Domain.EmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Service(long employmentCheckLastGetId);

        /// <summary>
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        public virtual async Task<IList<Models.Domain.EmploymentCheckModel>> GetApprenticeEmploymentChecks_Base(
            ILogger logger,
            string connectionString,
            string azureResource,
            int batchSize,
            long employmentCheckLastGetId,  // TODO: Review if this is needed, current solution is to use a db control table to store the Id of the last record read when populating the message queue
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticeEmploymentChecks_Base()";

            IList<Models.Domain.EmploymentCheckModel> EmploymentCheckModels = null;
            try
            {
                await using (var sqlConnection = await CreateSqlConnection(
                   logger,
                   connectionString,
                   azureResource,
                   azureServiceTokenProvider))
                {
                    if (sqlConnection != null)
                    {
                        await sqlConnection.OpenAsync();

                        // Get the stored Id of the last item in the previous batch from the EmploymentCheckControlTable (if this is the first call then nothing will be returned and it will use the passed in value of zero)
                        employmentCheckLastGetId = await GetEmploymentCheckLastHighestBatchId(logger, sqlConnection);

                        // Get a batch of records starting from the last highest id
                        EmploymentCheckModels = await GetEmploymentCheckBatch(logger, sqlConnection, batchSize, employmentCheckLastGetId);

                        if (EmploymentCheckModels != null &&
                            EmploymentCheckModels.Count > 0)
                        {
                            // Save the new highest batch id in the control table
                            employmentCheckLastGetId = EmploymentCheckModels.Max(aec => aec.Id);
                            if (employmentCheckLastGetId > 0)
                            {
                                var savedEmploymentCheckGetId = await SaveEmploymentCheckLastHighestBatchId(
                                    logger,
                                    sqlConnection,
                                    employmentCheckLastGetId);
                            }
                        }
                        else
                        {
                            logger.LogInformation($"{thisMethodName}: The database call to get the GetEmploymentCheckBatch returned [0] results.");
                            EmploymentCheckModels = new List<Models.Domain.EmploymentCheckModel>(); // return an empty result set rather than null which will report as an error in the calling method.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return EmploymentCheckModels;
        }

        private async Task<long> GetEmploymentCheckLastHighestBatchId(
            ILogger logger,
            SqlConnection sqlConnection)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckLastGetId()";

            long EmploymentCheckLastGetId = 0;
            try
            {
                // Get the stored Id of the last item in the previous batch stored in the EmploymentCheckControlTable (if this is the first call then nothing will be returned and it will use the passed in value of \ero)
                logger.LogInformation($"{thisMethodName}: Getting the EmploymentCheckLastHighestBatchId");
                var employmentCheckLastGetIdResult = await sqlConnection.QueryAsync<long>(
                  sql: "SELECT TOP (1) EmploymentCheckLastHighestBatchId FROM [Business].[EmploymentCheckControlTable] WHERE RowId = 1",
                  commandType: CommandType.Text);

                // Extract the employmentCheckLastGetId for the resultset (if there was a resultset)
                if (employmentCheckLastGetIdResult != null &&
                    employmentCheckLastGetIdResult.Any())
                {
                    // A list was returned so get the item in the list which is the row from the control table which has a column holding the Id of the highest Id retrieved in the previous batch
                    EmploymentCheckLastGetId = employmentCheckLastGetIdResult.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the EmploymentCheckLastGetId failed - {ex.Message}. {ex.StackTrace}");
            }

            return EmploymentCheckLastGetId;
        }

        private async Task<IList<Models.Domain.EmploymentCheckModel>> GetEmploymentCheckBatch(
            ILogger logger,
            SqlConnection sqlConnection,
            int batchSize,
            long employmentCheckLastGetId)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckBatch()";

            IEnumerable<Models.Domain.EmploymentCheckModel> apprenticeEmploymentChecksResult = null;
            IList<Models.Domain.EmploymentCheckModel> EmploymentCheckModels = null;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@batchSize", batchSize);
                parameters.Add("@employmentCheckLastGetId", employmentCheckLastGetId);

                // Get the next batch (or first batch if no previous batch) where the Id is greater than the stored EmploymentCheckLastGetId
                apprenticeEmploymentChecksResult = await sqlConnection.QueryAsync<EmploymentCheckModel>(
                sql: "SELECT TOP (@batchSize) " +
                "Id, " +
                "CorrelationId, " +
                "CheckType, " +
                "Uln, " +
                "ApprenticeshipId, " +
                "AccountId, " +
                "MinDate, " +
                "MaxDate, " +
                "Employed, " +
                "CreatedOn, " +
                "LastUpdated " +
                "FROM [Business].[EmploymentCheck] " +
                "WHERE Id > @employmentCheckLastHighestBatchId " +
                "ORDER BY Id",
                param: parameters,
                commandType: CommandType.Text);

                // Check we got some results
                if (apprenticeEmploymentChecksResult != null &&
                    apprenticeEmploymentChecksResult.Any())
                {
                    logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: Database query returned [{apprenticeEmploymentChecksResult.Count()}] apprentices requiring employment check.");

                    // ... and return the results
                    EmploymentCheckModels = apprenticeEmploymentChecksResult.Select(aec => new EmploymentCheckModel(
                        aec.Id,
                        aec.CorrelationId,
                        aec.CheckType,
                        aec.Uln,
                        aec.ApprenticeshipId,
                        aec.AccountId,
                        aec.MinDate,
                        aec.MaxDate,
                        aec.Employed,
                        aec.LastUpdated,
                        aec.CreatedOn)).ToList();
                }
                else
                {
                    logger.LogInformation($"{thisMethodName}: Database query returned [0] apprentices requiring employment check.");
                    EmploymentCheckModels = new List<Models.Domain.EmploymentCheckModel>(); // return an empty list rather than null
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the EmploymentCheckLastGetId failed - {ex.Message}. {ex.StackTrace}");
            }

            return EmploymentCheckModels;
        }

        private async Task<long> SaveEmploymentCheckLastHighestBatchId(
            ILogger logger,
            SqlConnection sqlConnection,
            long employmentCheckLastGetId)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckLastHighestBatchId()";

            long updatedEmploymentCheckLastGetId = 0;
            try
            {
                // On first use the control table will be empty and will need an insert rather than an update
                var rowCountResult = await sqlConnection.QueryAsync<long>(
                    sql: "SELECT COUNT(*) FROM [Business].[EmploymentChecksControlTable] WITH (NOLOCK)",
                    commandType: CommandType.Text);

                var rowCount = rowCountResult?.FirstOrDefault() ?? 0;

                if (rowCount > 0)
                {
                    // Update the existing row(s)
                    var parameters = new DynamicParameters();
                    parameters.Add("@employmentCheckLastHighestBatchId", employmentCheckLastGetId);

                    // Update the highest id of the batch as the starting point for the next batch
                    await sqlConnection.ExecuteAsync(
                       sql: "UPDATE [Business].[EmploymentChecksControlTable] SET EmploymentCheckLastHighestBatchId = @employmentCheckLastHighestBatchId]",
                       param: parameters,
                       commandType: CommandType.Text);
                }
                else
                {
                    // insert a row
                    // For the first call we set the employmentCheckLastGetId to zero to ensure we pick up the first batch
                    employmentCheckLastGetId = 0;

                    var parameters = new DynamicParameters();
                    parameters.Add("@employmentCheckLastHighestBatchId", employmentCheckLastGetId);

                    await sqlConnection.ExecuteAsync(
                       sql: "  INSERT [Business].[EmploymentChecksControlTable] ( " +
                       "RowId, " +
                       "EmploymentCheckLastHighestBatchId) " +
                       "VALUES (" +
                       "1, " +
                       "@employmentCheckLastHighestBatchId)",
                       param: parameters,
                       commandType: CommandType.Text);
                }

                // Check the saved value matches the value we saved
                updatedEmploymentCheckLastGetId = await GetEmploymentCheckLastHighestBatchId(logger, sqlConnection);

                if (updatedEmploymentCheckLastGetId != employmentCheckLastGetId)
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The updatedEmploymentCheckLastGetId [{updatedEmploymentCheckLastGetId}] value return from SaveTheEmploymentCheckLastGetId() does not match the value saved [{employmentCheckLastGetId}].");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the EmploymentCheckLastGetId failed - {ex.Message}. {ex.StackTrace}");
            }

            return updatedEmploymentCheckLastGetId;
        }

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue
        /// Note: This abstract class makes it a requirement to implement this method in the Service and the ServiceStub and for local testing the ServiceStub will
        /// call the implementation defined method below this one
        /// </summary>
        /// <param name="apprenticeEmploymentData"></param>
        /// <returns></returns>
        public abstract Task EnqueueApprenticeEmploymentCheckMessages_Service(EmploymentCheckData apprenticeEmploymentData);

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="apprenticeEmploymentData"></param>
        /// <returns>Task</returns>
        public virtual async Task EnqueueApprenticeEmploymentCheckMessages_Service(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            EmploymentCheckData apprenticeEmploymentData)
        {
            var thisMethodName = $"{ThisClassName}.EnqueueApprenticeEmploymentCheckMessages_Service()";

            try
            {
                if (apprenticeEmploymentData != null)
                {
                    var apprenticeEmploymentCheckMessages = await CreateEmploymentCheckMessages(logger, apprenticeEmploymentData);

                    if(apprenticeEmploymentCheckMessages != null)
                    {
                        await StoreApprenticeEmploymentCheckMessages(
                            logger,
                            connectionString,
                            azureResource,
                            azureServiceTokenProvider,
                            apprenticeEmploymentCheckMessages);
                    }
                    else
                    {
                        logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessages value returned from StoreApprenticeEmploymentCheckMessages() is null.");
                    }
                }
                else
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentData input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API
        /// Note: This abstract class makes it a requirement to implement this method in the Service and the ServiceStub and for local testing the ServiceStub will
        /// call the implementation defined method below this one
        /// </summary>
        /// <returns></returns>
        public abstract Task<EmploymentCheckMessage> DequeueApprenticeEmploymentCheckMessage_Service();

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<ApprenticeEmploymentCheckMessageModel></returns>
        public virtual async Task<EmploymentCheckMessage> DequeueApprenticeEmploymentCheckMessage_Base(
            ILogger logger,
            string connectionString,
            int batchSize,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.DequeueApprenticeEmploymentCheckMessage_Base()";

            EmploymentCheckMessage apprenticeEmploymentCheckMessageModel = null;
            try
            {
                apprenticeEmploymentCheckMessageModel = await GetApprenticeEmploymentCheckMessage_Base(
                    logger,
                    connectionString,
                    azureResource,
                    batchSize,
                    azureServiceTokenProvider);

                if (apprenticeEmploymentCheckMessageModel == null)
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModel value returned from the call to GetApprenticeEmploymentCheckMessage_Base() is null.");
                }

            }
            catch (Exception ex)
            {
                logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticeEmploymentCheckMessageModel;
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
        public virtual async Task<EmploymentCheckMessage> GetApprenticeEmploymentCheckMessage_Base(
              ILogger logger,
              string connectionString,
              string azureResource,
              int batchSize,
              AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticeEmploymentCheckMessage_Base()";

            EmploymentCheckMessage employmentCheckMessage = null;
            SqlConnection sqlConnection = null;

            try
            {
                await using (sqlConnection = await CreateSqlConnection(
                    logger,
                    connectionString,
                    azureResource,
                    azureServiceTokenProvider))
                {
                    if (sqlConnection != null)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@batchSize", batchSize);

                        await sqlConnection.OpenAsync();
                        // TODO: The expectation is that there is only one instance of this code getting the message from the database.
                        //       If there are going to be multiple instances of the code pulling the messages off the message queue
                        //       then we will need to make the select/delete of the message transactional to lock the row so that
                        //       other instances aren't pulling the same message
                        employmentCheckMessage =
                            (await sqlConnection.QueryAsync<EmploymentCheckMessage>(
                                sql:
                                "SELECT TOP (1) " +
                                "Id, " +
                                "EmploymentCheckId, " +
                                "CorrelationId, " +
                                "Uln, " +
                                "NationalInsuranceNumber, " +
                                "PayeScheme, " +
                                "MinDateTime, " +
                                "MaxDateTime, " +
                                "LastEmploymentCheck, " +
                                "Employed, " +
                                "ResponseId, " +
                                "ResponseMessage, " +
                                "CreatedOn " +
                                "FROM [Cache].[EmploymentCheckMessageQueue] " +
                                "ORDER BY Id",
                                param: parameters,
                                commandType: CommandType.Text)).FirstOrDefault();

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
            catch (Exception ex)
            {
                logger.LogError(
                    $"{thisMethodName} {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckMessage;
        }

        public abstract Task SaveEmploymentCheckResult_Service(EmploymentCheckMessage apprenticeEmploymentCheckMessageModel);

        public virtual async Task SaveEmploymentCheckResult_Base(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            EmploymentCheckMessage employmentCheckMessageModel)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckResult_Base()";

            try
            {
                if (employmentCheckMessageModel != null)
                {
                    var sqlConnection = await CreateSqlConnection(logger, connectionString, azureResource, azureServiceTokenProvider);
                    if (sqlConnection == null)
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
                                employmentCheckParameters.Add("id", employmentCheckMessageModel.EmploymentCheckId, DbType.Int64);
                                employmentCheckParameters.Add("messageId", employmentCheckMessageModel.Id, DbType.Int64);
                                employmentCheckParameters.Add("employed", employmentCheckMessageModel.Employed, DbType.Boolean);
                                employmentCheckParameters.Add("hasBeenChecked", true);

                                await sqlConnection.ExecuteAsync(
                                    "UPDATE [Business].[EmploymentCheck] " +
                                    "SET Employed = @employed, " +
                                    "LastUpdated = GETDATE(), " +
                                    "WHERE Id = @id",
                                        employmentCheckParameters,
                                        commandType: CommandType.Text,
                                        transaction: transaction);

                                // -------------------------------------------------------------------
                                // Archive a copy of the employment check message for auditing
                                // -------------------------------------------------------------------
                                var employmentCheckMessageHistoryModel = new EmploymentCheckMessageHistory(employmentCheckMessageModel);

                                var messageHistoryParameters = new DynamicParameters();
                                messageHistoryParameters.Add("@messageId", employmentCheckMessageHistoryModel.MessageId, DbType.Int64);
                                messageHistoryParameters.Add("@employmentCheckId", employmentCheckMessageHistoryModel.EmploymentCheckId, DbType.Int64);
                                messageHistoryParameters.Add("@correlationId", employmentCheckMessageHistoryModel.CorrelationId, DbType.Int64);
                                messageHistoryParameters.Add("@uln", employmentCheckMessageHistoryModel.Uln, DbType.Int64);
                                messageHistoryParameters.Add("@nationalInsuranceNumber", employmentCheckMessageHistoryModel.NationalInsuranceNumber, DbType.String);
                                messageHistoryParameters.Add("@payeScheme", employmentCheckMessageHistoryModel.PayeScheme, DbType.String);
                                messageHistoryParameters.Add("@minDateTime", employmentCheckMessageHistoryModel.MinDateTime, DbType.DateTime);
                                messageHistoryParameters.Add("@maxDateTime", employmentCheckMessageHistoryModel.MaxDateTime, DbType.DateTime);
                                messageHistoryParameters.Add("@employed", employmentCheckMessageHistoryModel.Employed ?? false, DbType.Boolean);
                                messageHistoryParameters.Add("@lastEmploymentCheck", employmentCheckMessageHistoryModel.LastEmploymentCheck, DbType.DateTime);
                                messageHistoryParameters.Add("@responseId", employmentCheckMessageHistoryModel.ResponseId, DbType.Int16);
                                messageHistoryParameters.Add("@responseMessage", employmentCheckMessageHistoryModel.ResponseMessage, DbType.String);
                                messageHistoryParameters.Add("@messageCreatedOn", employmentCheckMessageHistoryModel.MessageCreatedOn, DbType.DateTime);
                                messageHistoryParameters.Add("@createdOn", employmentCheckMessageHistoryModel.CreatedOn, DbType.DateTime);

                                await sqlConnection.ExecuteAsync(
                                    "INSERT [SFA.DAS.EmploymentCheck.Database].[Cache].[EmploymentCheckMessageQueueHistory] " +
                                    "( " +
                                    ",[MessageId] " +
                                    ",[EmploymentCheckId] " +
                                    ",[CorrelationId] " +
                                    ",[Uln] " +
                                    ",[NationalInsuranceNumber] " +
                                    ",[PayeScheme] " +
                                    ",[MinDateTime] " +
                                    ",[MaxDateTime] " +
                                    ",[Employed] " +
                                    ",[LastEmploymentChecked] " +
                                    ",[ResponseId] " +
                                    ",[ResponseMessage] " +
                                    ",[MessageCreatedOn] " +
                                    ",[CreatedOn] " +
                                    ") " +
                                    "VALUES (" +
                                    ",@messageId " +
                                    ",@employmentCheckId " +
                                    ",@correlationId " +
                                    ",@uln " +
                                    ",@nationalInsuranceNumber " +
                                    ",@payeScheme " +
                                    ",@minDateTime " +
                                    ",@maxDateTime" +
                                    ",@employed " +
                                    ",@lastEmploymentCheck " +
                                    ",@responseId " +
                                    ",@responseMessage ",
                                    ",@messageCreatedOn " +
                                    ",@createdOn)" +
                                    messageHistoryParameters,
                                    commandType: CommandType.Text,
                                    transaction: transaction);

                                // -------------------------------------------------------------------
                                // Remove the employment check message from the queue
                                // -------------------------------------------------------------------
                                var messageParameters = new DynamicParameters();
                                messageParameters.Add("messageId", employmentCheckMessageModel.Id, DbType.Int64);

                                await sqlConnection.ExecuteAsync(
                                    "DELETE [Cache].[EmploymentCheckMessageQueue] WHERE Id = @id",
                                        messageParameters,
                                        commandType: CommandType.Text,
                                        transaction: transaction);

                                // -------------------------------------------------------------------
                                // Commit the transaction
                                // -------------------------------------------------------------------
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
                            }
                        }
                    }
                }
                else
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModel input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        public virtual async Task<IList<EmploymentCheckMessage>> CreateEmploymentCheckMessages(
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
                    // Create an ApprenticeEmploymentCheckMessage for each combination of Uln, National Insurance Number and PayeSchemes.
                    // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln and National Insurance Number
                    // Note: Once we get a 'hit' for a given Nino, PayeScheme, StartDate, EndDate from the HMRC API we could discard the remaining messages without calling the API if necessary
                    // (at this point we don't know if we need to check all the payeschemes for a given apprentice just to ensure there's no match on multiple schemes)
                    foreach (var apprentice in employmentCheckData.EmploymentChecks)
                    {
                        var apprenticeEmploymentCheckMessage = new EmploymentCheckMessage();

                        // Get the National Insurance Number for this apprentice
                        var nationalInsuranceNumber = employmentCheckData.ApprenticeNiNumbers.Where(ninumber => ninumber.Uln == apprentice.Uln).FirstOrDefault().NationalInsuranceNumber;

                        // Get the employer paye schemes for this apprentices
                        var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.Where(ps => ps.EmployerAccountId == apprentice.AccountId).FirstOrDefault();

                        // Create the individual message combinations for each paye scheme
                        foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                        {
                            apprenticeEmploymentCheckMessage.EmploymentCheckId = apprentice.Id;
                            apprenticeEmploymentCheckMessage.Uln = apprentice.Uln;
                            apprenticeEmploymentCheckMessage.NationalInsuranceNumber = nationalInsuranceNumber;
                            apprenticeEmploymentCheckMessage.MinDateTime = apprentice.MinDate;
                            apprenticeEmploymentCheckMessage.MaxDateTime = apprentice.MinDate;
                            apprenticeEmploymentCheckMessage.PayeScheme = payeScheme;

                            employmentCheckMessages.Add(apprenticeEmploymentCheckMessage);
                        }
                    }
                }
                else
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The Apprentice Employment Data supplied to create Apprentice Employment Check Messages is incomplete.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(employmentCheckMessages);
        }

        /// <summary>
        /// Creates the individual apprentice employment messages and stores them in the database table queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="apprenticeEmploymentCheckMessages"></param>
        /// <returns>Task</returns>
        public virtual async Task StoreApprenticeEmploymentCheckMessages(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            IList<EmploymentCheckMessage> apprenticeEmploymentCheckMessages)
        {
            var thisMethodName = $"{ThisClassName}.StoreApprenticeEmploymentCheckMessage()";

            try
            {
                if (apprenticeEmploymentCheckMessages != null &&
                    apprenticeEmploymentCheckMessages.Any())
                {
                    await using (var sqlConnection = await CreateSqlConnection(
                        logger,
                        connectionString,
                        azureResource,
                        azureServiceTokenProvider))
                    {
                        if (sqlConnection != null)
                        {
                            await sqlConnection.OpenAsync();

                            foreach (var apprenticeEmploymentCheckMessage in apprenticeEmploymentCheckMessages)
                            {
                                await StoreApprenticeEmploymentCheckMessage(
                                    logger,
                                    connectionString,
                                    sqlConnection,
                                    azureResource,
                                    azureServiceTokenProvider,
                                    apprenticeEmploymentCheckMessage);
                            }
                        }
                        else
                        {
                            logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Creation of SQL Connection for the Employment Check Databasse failed.");
                        }
                    }
                }
                else
                {
                    logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No apprentice employment check queue messaages to store.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Store an individual employment check message in the database table queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="employmentCheckMessageModel"></param>
        /// <returns>Task</returns>
        public virtual async Task StoreApprenticeEmploymentCheckMessage(
            ILogger logger,
            string connectionString,
            SqlConnection sqlConnection,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            EmploymentCheckMessage employmentCheckMessageModel)
        {
            var thisMethodName = $"{ThisClassName}.StoreApprenticeEmploymentCheckMessage()";

            try
            {
                if (employmentCheckMessageModel != null)
                {
                    if (sqlConnection == null)
                    {
                        sqlConnection = await CreateSqlConnection(
                            logger,
                            connectionString,
                            azureResource,
                            azureServiceTokenProvider);

                        await sqlConnection.OpenAsync();
                    }

                    var employmentCheckMessageQueueParameter = new DynamicParameters();
                    employmentCheckMessageQueueParameter.Add("@employmentCheckId", employmentCheckMessageModel.EmploymentCheckId, DbType.Int64);
                    employmentCheckMessageQueueParameter.Add("@correlationId", employmentCheckMessageModel.CorrelationId, DbType.Int64);
                    employmentCheckMessageQueueParameter.Add("@uln", employmentCheckMessageModel.Uln, DbType.Int64);
                    employmentCheckMessageQueueParameter.Add("@nationalInsuranceNumber", employmentCheckMessageModel.NationalInsuranceNumber, DbType.String);
                    employmentCheckMessageQueueParameter.Add("@payeScheme", employmentCheckMessageModel.PayeScheme, DbType.String);
                    employmentCheckMessageQueueParameter.Add("@minDateTime", employmentCheckMessageModel.MinDateTime, DbType.DateTime);
                    employmentCheckMessageQueueParameter.Add("@maxDateTime", employmentCheckMessageModel.MaxDateTime, DbType.DateTime);
                    employmentCheckMessageQueueParameter.Add("@employed", employmentCheckMessageModel.Employed ?? false, DbType.Boolean);
                    employmentCheckMessageQueueParameter.Add("@lastEmploymentCheck", employmentCheckMessageModel.LastEmploymentCheck, DbType.DateTime);
                    employmentCheckMessageQueueParameter.Add("@responseId", employmentCheckMessageModel.ResponseId, DbType.Int16);
                    employmentCheckMessageQueueParameter.Add("@responseMessage", employmentCheckMessageModel.ResponseMessage, DbType.String);
                    employmentCheckMessageQueueParameter.Add("@createdOn", employmentCheckMessageModel.CreatedOn, DbType.DateTime);

                    await sqlConnection.ExecuteAsync(
                        "INSERT [SFA.DAS.EmploymentCheck.Database].[Cache].[EmploymentCheckMessageQueue] " +
                        "( " +
                        ",[EmploymentCheckId] " +
                        ",[CorrelationId] " +
                        ",[Uln] " +
                        ",[NationalInsuranceNumber] " +
                        ",[PayeScheme] " +
                        ",[MinDateTime] " +
                        ",[MaxDateTime] " +
                        ",[Employed] " +
                        ",[LastEmploymentChecked] " +
                        ",[ResponseId] " +
                        ",[ResponseMessage] " +
                        ",[LastUpdated] " +
                        ",[CreatedOn] " +
                        ") " +
                        "VALUES (" +
                        ",@employmentCheckId " +
                        ",@correlationId " +
                        ",@uln " +
                        ",@nationalInsuranceNumber " +
                        ",@payeScheme " +
                        ",@minDateTime " +
                        ",@maxDateTime" +
                        ",@employed " +
                        ",@lastEmploymentCheck " +
                        ",@ResponseId " +
                        ",@ResponseMessage ",
                        ",@createdOn)" +
                        employmentCheckMessageQueueParameter,
                        commandType: CommandType.Text);
                }
                else
                {
                    logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No apprentice employment check queue messaages to store.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        public virtual async Task<SqlConnection> CreateSqlConnection(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.CreateConnection()";

            SqlConnection sqlConnection = null;
            try
            {
                if (!String.IsNullOrEmpty(connectionString))
                {
                    if (!String.IsNullOrEmpty(azureResource))
                    {
                        sqlConnection = new SqlConnection(connectionString);
                        if (sqlConnection != null)
                        {
                            // no token for local db
                            if (azureServiceTokenProvider != null)
                            {
                                sqlConnection.AccessToken = await azureServiceTokenProvider.GetAccessTokenAsync(azureResource);
                            }
                            else
                            {
                                logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Creation of SQL Connection for the Employment Check Databasse failed.");
                            }
                        }
                        else
                        {
                            logger.LogInformation($"{thisMethodName}: Missing AzureResource string for the Employment Check Databasse.");
                        }
                    }
                    else
                    {
                        logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Missing SQL connecton string for the Employment Check Databasse.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return sqlConnection;
        }

        public abstract Task SeedEmploymentCheckApprenticeDatabaseTableTestData();


        public virtual async Task SeedEmploymentCheckApprenticeDatabaseTableTestData(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.SeedEmploymentCheckApprenticeDatabaseTableTestData()";

            try
            {
                await using (var connection = await CreateSqlConnection(
                    logger,
                    connectionString,
                    azureResource,
                    azureServiceTokenProvider))
                {
                    if (connection != null)
                    {
                        await connection.OpenAsync();

                        var i = 0;
                        foreach (var input in EmploymentCheckServiceStub.StubApprenticeEmploymentCheckMessageData)
                        {
                            i++;
                            var now = DateTime.Now;
                            var parameters = new DynamicParameters();
                            parameters.Add("@correlationId", 1000000000 + i, DbType.Int64);
                            parameters.Add("@checkType", "StartDate+60", DbType.String);
                            parameters.Add("@uln", 1000000000 + i, DbType.Int64);
                            parameters.Add("@apprenticeshipId", i, DbType.Int64);
                            parameters.Add("@accountId", i, DbType.Int64);
                            parameters.Add("@minDate", input.MinDateTime, DbType.DateTime);
                            parameters.Add("@maxDate", input.MaxDateTime, DbType.DateTime);
                            parameters.Add("@employed", false, DbType.Boolean);
                            parameters.Add("@lastUpdated", now, DbType.DateTime);
                            parameters.Add("@createdOn", now, DbType.DateTime);


                            await connection.ExecuteAsync(
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
                        logger.LogInformation(
                            $"\n\n{DateTime.UtcNow} {thisMethodName}: *** ERROR ***: Creation of SQL Connection for the Employment Check Databasse failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}