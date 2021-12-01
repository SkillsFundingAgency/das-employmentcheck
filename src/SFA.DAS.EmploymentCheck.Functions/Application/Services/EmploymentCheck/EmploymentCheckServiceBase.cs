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
    public abstract class EmploymentCheckServiceBase
        : IEmploymentCheckService
    {
        private const string ThisClassName = "\n\nEmploymentCheckServiceBase";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        // The EmploymentCheckService and EmploymentCheckServiceStub can implement this method to either call the database code or the stub code as appropriate
        public abstract Task<IList<Models.Domain.EmploymentCheckModel>> GetEmploymentChecksBatch_Service(long employmentCheckLastHighestBatchId);

        /// <summary>
        /// Gets a batch of the employment checks from the Employment Check database
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        public virtual async Task<IList<Models.Domain.EmploymentCheckModel>> GetEmploymentChecks_Base(
            ILogger logger,
            string connectionString,
            string azureResource,
            int batchSize,
            long employmentCheckLastHighestBatchId,  // TODO: Review if this is needed, current solution is to use a db control table to store the Id of the last record read when populating the message queue
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentChecks_Base()";

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
                        employmentCheckLastHighestBatchId = await GetEmploymentCheckLastHighestBatchId(logger, sqlConnection);

                        // Get a batch of records starting from the last highest id
                        EmploymentCheckModels = await GetEmploymentCheckBatch(logger, sqlConnection, batchSize, employmentCheckLastHighestBatchId);

                        if (EmploymentCheckModels != null &&
                            EmploymentCheckModels.Count > 0)
                        {
                            // Save the new highest batch id in the control table
                            employmentCheckLastHighestBatchId = EmploymentCheckModels.Max(aec => aec.Id);
                            if (employmentCheckLastHighestBatchId > 0)
                            {
                                var savedEmploymentCheckGetId = await SaveEmploymentCheckLastHighestBatchId(
                                    logger,
                                    sqlConnection,
                                    employmentCheckLastHighestBatchId);
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
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckLastHighestBatchId()";

            long employmentCheckLastHighestBatchId = 0;
            try
            {
                // Get the stored Id of the last item in the previous batch stored in the EmploymentCheckControlTable (if this is the first call then nothing will be returned and it will use the passed in value of \ero)
                logger.LogInformation($"{thisMethodName}: Getting the EmploymentCheckLastHighestBatchId");
                var employmentCheckLastHighestBatchIdResult = await sqlConnection.QueryAsync<long>(
                  sql: "SELECT TOP (1) EmploymentCheckLastHighestBatchId FROM [Business].[EmploymentCheckControlTable] WHERE RowId = 1",
                  commandType: CommandType.Text);

                // Extract the employmentCheckLastHighestBatchId for the resultset (if there was a resultset)
                if (employmentCheckLastHighestBatchIdResult != null &&
                    employmentCheckLastHighestBatchIdResult.Any())
                {
                    // A list was returned so get the item in the list which is the row from the control table which has a column holding the Id of the highest Id retrieved in the previous batch
                    employmentCheckLastHighestBatchId = employmentCheckLastHighestBatchIdResult.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the employmentCheckLastHighestBatchId failed - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckLastHighestBatchId;
        }

        private async Task<IList<Models.Domain.EmploymentCheckModel>> GetEmploymentCheckBatch(
            ILogger logger,
            SqlConnection sqlConnection,
            int batchSize,
            long employmentCheckLastHighestBatchId)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckBatch()";

            IEnumerable<Models.Domain.EmploymentCheckModel> employmentChecksResult = null;
            IList<Models.Domain.EmploymentCheckModel> employmentCheckModels = null;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@batchSize", batchSize);
                parameters.Add("@employmentCheckLastHighestBatchId", employmentCheckLastHighestBatchId);

                // Get the next batch (or first batch if no previous batch) where the Id is greater than the stored employmentCheckLastHighestBatchId
                employmentChecksResult = await sqlConnection.QueryAsync<EmploymentCheckModel>(
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
                if (employmentChecksResult != null &&
                    employmentChecksResult.Any())
                {
                    logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: Database query returned [{employmentChecksResult.Count()}] employment checks.");

                    // ... and return the results
                    employmentCheckModels = employmentChecksResult.Select(aec => new EmploymentCheckModel(
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
                    logger.LogError($"{thisMethodName}: Database query returned [0] employment checks.");
                    employmentCheckModels = new List<EmploymentCheckModel>(); // return an empty list rather than null
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the employmentCheckLastHighestBatchId failed - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckModels;
        }

        private async Task<long> SaveEmploymentCheckLastHighestBatchId(
            ILogger logger,
            SqlConnection sqlConnection,
            long employmentCheckLastHighestBatchId)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckLastHighestBatchId()";

            long updatedEmploymentCheckLastHighestBatchId = 0;
            try
            {
                // On first use the control table will be empty and will need an insert rather than an update
                var rowCountResult = await sqlConnection.QueryAsync<long>(
                    sql: "SELECT COUNT(*) FROM [Business].[EmploymentCheckControlTable] WITH (NOLOCK)",
                    commandType: CommandType.Text);

                var rowCount = rowCountResult?.FirstOrDefault() ?? 0;

                if (rowCount > 0)
                {
                    // Update the existing row(s)
                    var parameters = new DynamicParameters();
                    parameters.Add("@employmentCheckLastHighestBatchId", employmentCheckLastHighestBatchId);

                    // Update the highest id of the batch as the starting point for the next batch
                    await sqlConnection.ExecuteAsync(
                       sql: "UPDATE [Business].[EmploymentCheckControlTable] SET EmploymentCheckLastHighestBatchId = @employmentCheckLastHighestBatchId]",
                       param: parameters,
                       commandType: CommandType.Text);
                }
                else
                {
                    // insert a row
                    // For the first call we set the employmentCheckLastHighestBatchId to zero to ensure we pick up the first batch
                    employmentCheckLastHighestBatchId = 0;

                    var parameters = new DynamicParameters();
                    parameters.Add("@employmentCheckLastHighestBatchId", employmentCheckLastHighestBatchId);

                    await sqlConnection.ExecuteAsync(
                       sql: "  INSERT [Business].[EmploymentCheckControlTable] ( " +
                       "RowId, " +
                       "EmploymentCheckLastHighestBatchId) " +
                       "VALUES (" +
                       "1, " +
                       "@employmentCheckLastHighestBatchId)",
                       param: parameters,
                       commandType: CommandType.Text);
                }

                // Check the saved value matches the value we saved
                updatedEmploymentCheckLastHighestBatchId = await GetEmploymentCheckLastHighestBatchId(logger, sqlConnection);

                if (updatedEmploymentCheckLastHighestBatchId != employmentCheckLastHighestBatchId)
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The updatedEmploymentCheckLastHighestBatchId [{updatedEmploymentCheckLastHighestBatchId}] value return from SaveTheEmploymentCheckLastHighestBatchId() does not match the value saved [{employmentCheckLastHighestBatchId}].");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the EmploymentCheckLastHighestBatchId failed - {ex.Message}. {ex.StackTrace}");
            }

            return updatedEmploymentCheckLastHighestBatchId;
        }

        /// <summary>
        /// Adds an employment check message to the HMRC API message queue
        /// Note: This abstract class makes it a requirement to implement this method in the Service and the ServiceStub and for local testing the ServiceStub will
        /// call the implementation defined method below this one
        /// </summary>
        /// <param name="EmploymentCheckData"></param>
        /// <returns></returns>
        public abstract Task EnqueueEmploymentCheckMessages_Service(EmploymentCheckData employmentCheckData);

        /// <summary>
        /// Adds an employment check message to the HMRC API message queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="EmploymentCheckData"></param>
        /// <returns>Task</returns>
        public virtual async Task EnqueueEmploymentCheckMessages_Service(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{ThisClassName}.EnqueueEmploymentCheckMessages_Service()";

            try
            {
                if (employmentCheckData != null)
                {
                    var employmentCheckMessages = await CreateEmploymentCheckMessages(logger, employmentCheckData);

                    if(employmentCheckMessages != null)
                    {
                        await StoreEmploymentCheckMessages(
                            logger,
                            connectionString,
                            azureResource,
                            azureServiceTokenProvider,
                            employmentCheckMessages);
                    }
                    else
                    {
                        logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessages value returned from StoreEmploymentCheckMessages() is null.");
                    }
                }
                else
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckData input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets an employment check message from the HMRC API message queue
        /// Note: This abstract class makes it a requirement to implement this method in the Service and the ServiceStub and for local testing the ServiceStub will
        /// call the implementation defined method below this one
        /// </summary>
        /// <returns></returns>
        public abstract Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage_Service();

        /// <summary>
        /// Gets an employment check message from the HMRC API message queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<EmploymentCheckMessage></returns>
        public virtual async Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage_Base(
            ILogger logger,
            string connectionString,
            int batchSize,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.DequeueEmploymentCheckMessage_Base()";

            EmploymentCheckMessage employmentCheckMessage = null;
            try
            {
                employmentCheckMessage = await GetEmploymentCheckMessage_Base(
                    logger,
                    connectionString,
                    azureResource,
                    batchSize,
                    azureServiceTokenProvider);

                if (employmentCheckMessage == null)
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessage value returned from the call to GetEmploymentCheckMessage_Base() is null.");
                }

            }
            catch (Exception ex)
            {
                logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
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
        public virtual async Task<EmploymentCheckMessage> GetEmploymentCheckMessage_Base(
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
                await using (sqlConnection = await CreateSqlConnection(
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
                                           "MessageId, " +
                                           "MessageCreatedDateTime, " +
                                           "EmploymentCheckId, " +
                                           "Uln, " +
                                           "NationalInsuranceNumber, " +
                                           "PayeScheme, " +
                                           "StartDateTime, " +
                                           "EndDateTime, " +
                                           "EmploymentCheckedDateTime, " +
                                           "IsEmployed, " +
                                           "ReturnCode, " +
                                           "ReturnMessage " +
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
            catch (Exception ex)
            {
                logger.LogError(
                    $"{thisMethodName} {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckMessage;
        }

        public abstract Task SaveEmploymentCheckResult_Service(EmploymentCheckMessage employmentCheckMessage);

        public virtual async Task SaveEmploymentCheckResult_Base(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckResult_Base()";

            try
            {
                if (employmentCheckMessage != null)
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
                                employmentCheckParameters.Add("id", employmentCheckMessage.EmploymentCheckId, DbType.Int64);
                                employmentCheckParameters.Add("messageId", employmentCheckMessage.Id, DbType.Int64);
                                employmentCheckParameters.Add("employed", employmentCheckMessage.Employed, DbType.Boolean);
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
                                var employmentCheckMessageHistoryModel = new EmploymentCheckMessageHistory(employmentCheckMessage);

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
                                messageParameters.Add("messageId", employmentCheckMessage.Id, DbType.Int64);

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
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessage input parameter is null.");
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
                    // Create an EmploymentCheckMessage for each combination of Uln, National Insurance Number and PayeSchemes.
                    // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln and National Insurance Number
                    // Note: Once we get a 'match' for a given Nino, PayeScheme, StartDate, EndDate from the HMRC API we could discard the remaining messages without calling the API if necessary
                    // (at this point we don't know if we need to check all the payeschemes for a given apprentice just to ensure there's no match on multiple schemes)
                    foreach (var employmentCheck in employmentCheckData.EmploymentChecks)
                    {
                        var employmentCheckMessage = new EmploymentCheckMessage();

                        // Get the National Insurance Number for this apprentice
                        var nationalInsuranceNumber = employmentCheckData.ApprenticeNiNumbers.Where(ninumber => ninumber.Uln == employmentCheck.Uln).FirstOrDefault().NationalInsuranceNumber;

                        // Get the employer paye schemes for this apprentices
                        var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.Where(ps => ps.EmployerAccountId == employmentCheck.AccountId).FirstOrDefault();

                        // Create the individual message combinations for each paye scheme
                        foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                        {
                            employmentCheckMessage.EmploymentCheckId = employmentCheck.Id;
                            employmentCheckMessage.Uln = employmentCheck.Uln;
                            employmentCheckMessage.F = nationalInsuranceNumber;
                            employmentCheckMessage.MinDateTime = employmentCheck.MinDate;
                            employmentCheckMessage.MaxDateTime = employmentCheck.MinDate;
                            employmentCheckMessage.PayeScheme = payeScheme;

                            employmentCheckMessages.Add(employmentCheckMessage);
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

        /// <summary>
        /// Creates the individual employment check messages and stores them in the database table queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="EmploymentCheckMessages"></param>
        /// <returns>Task</returns>
        public virtual async Task StoreEmploymentCheckMessages(
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
                    await using (var sqlConnection = await CreateSqlConnection(
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
                                await StoreEmploymentCheckMessage(
                                    logger,
                                    connectionString,
                                    sqlConnection,
                                    azureResource,
                                    azureServiceTokenProvider,
                                    employmentCheckMessage);
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
                    logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No employment check queue messaages to store.");
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
        public virtual async Task StoreEmploymentCheckMessage(
            ILogger logger,
            string connectionString,
            SqlConnection sqlConnection,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            EmploymentCheckMessage employmentCheckMessageModel)
        {
            var thisMethodName = $"{ThisClassName}.StoreEmploymentCheckMessage()";

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
                    logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No employment check queue messaages to store.");
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
                                logger.LogInformation($"{thisMethodName}: azureServiceTokenProvider was not specified, call to azureServiceTokenProvider.GetAccessTokenAsync(azureResource) skipped.");
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

        public abstract Task SeedEmploymentCheckDatabaseTableTestData();


        public virtual async Task SeedEmploymentCheckDatabaseTableTestData(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.SeedEmploymentCheckDatabaseTableTestData()";

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
                        foreach (var input in EmploymentCheckServiceStub.StubEmploymentCheckMessageData)
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