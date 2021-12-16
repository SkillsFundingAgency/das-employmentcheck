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

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public abstract class EmploymentCheckServiceBase
        : IEmploymentCheckService
    {
        private const string ThisClassName = "\n\nEmploymentCheckServiceBase";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        // The EmploymentCheckService and EmploymentCheckServiceStub can implement this method to either call the database code or the stub code as appropriate
        public abstract Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Service(long employmentCheckLastGetId);

        /// <summary>
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database
        /// </summary>
        public virtual async Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecks_Base(
            ILogger logger,
            string connectionString,
            string azureResource,
            int batchSize,
            long employmentCheckLastGetId,  // TODO: Review if this is needed, current solution is to use a db control table to store the Id of the last record read when populating the message queue
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticeEmploymentChecks_Base()";

            IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentCheckModels = null;
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
                        employmentCheckLastGetId = await GetEmploymentCheckLastGetId(logger, sqlConnection);

                        // Get a batch of records starting from the last highest id
                        apprenticeEmploymentCheckModels = await GetEmploymentCheckBatch(logger, sqlConnection, batchSize, employmentCheckLastGetId);

                        if (apprenticeEmploymentCheckModels != null &&
                            apprenticeEmploymentCheckModels.Count > 0)
                        {
                            // Save the new highest batch id in the control table
                            employmentCheckLastGetId = apprenticeEmploymentCheckModels.Max(aec => aec.Id);
                            if (employmentCheckLastGetId > 0)
                            {
                                var savedEmploymentCheckGetId = await SaveTheEmploymentCheckLastGetId(
                                    logger,
                                    sqlConnection,
                                    employmentCheckLastGetId);
                            }
                        }
                        else
                        {
                            logger.LogInformation($"{thisMethodName}: The database call to get the GetEmploymentCheckBatch returned [0] results.");
                            apprenticeEmploymentCheckModels = new List<ApprenticeEmploymentCheckModel>(); // return an empty result set rather than null which will report as an error in the calling method.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticeEmploymentCheckModels;
        }

        private async Task<long> GetEmploymentCheckLastGetId(
            ILogger logger,
            SqlConnection sqlConnection)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckLastGetId()";

            long EmploymentCheckLastGetId = 0;
            try
            {
                // Get the stored Id of the last item in the previous batch stored in the EmploymentCheckControlTable (if this is the first call then nothing will be returned and it will use the passed in value of \ero)
                logger.LogInformation($"{thisMethodName}: Getting the EmploymentCheckLastGetId");
                var employmentCheckLastGetIdResult = await sqlConnection.QueryAsync<long>(
                  sql: "SELECT TOP (1) EmploymentCheckLastGetId FROM [dbo].[EmploymentChecksControlTable] WHERE RowId = 1",
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

        private async Task<IList<ApprenticeEmploymentCheckModel>> GetEmploymentCheckBatch(
            ILogger logger,
            SqlConnection sqlConnection,
            int batchSize,
            long employmentCheckLastGetId)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckBatch()";

            IEnumerable<ApprenticeEmploymentCheckModel> apprenticeEmploymentChecksResult = null;
            IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentCheckModels = null;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@batchSize", batchSize);
                parameters.Add("@employmentCheckLastGetId", employmentCheckLastGetId);

                // Get the next batch (or first batch if no previous batch) where the Id is greater than the stored EmploymentCheckLastGetId
                apprenticeEmploymentChecksResult = await sqlConnection.QueryAsync<ApprenticeEmploymentCheckModel>(
                sql: "SELECT TOP (@batchSize) Id," +
                        "ULN," +
                        "NationalInsuranceNumber, " +
                        "UKPRN," +
                        "ApprenticeshipId," +
                        "AccountId," +
                        "MinDate," +
                        "MaxDate," +
                        "CheckType," +
                        "IsEmployed," +
                        "LastUpdated," +
                        "CreatedDate," +
                        "HasBeenChecked " +
                        "FROM [dbo].[EmploymentChecks] " +
                        "WHERE Id > @employmentCheckLastGetId " +
                        "AND HasBeenChecked = 0 " +
                        "ORDER BY Id ASC",
                param: parameters,
                commandType: CommandType.Text);

                // Check we got some results
                if (apprenticeEmploymentChecksResult != null &&
                    apprenticeEmploymentChecksResult.Any())
                {
                    logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: Database query returned [{apprenticeEmploymentChecksResult.Count()}] apprentices requiring employment check.");

                    // ... and return the results
                    apprenticeEmploymentCheckModels = apprenticeEmploymentChecksResult.Select(aec => new ApprenticeEmploymentCheckModel(
                        aec.Id,
                        aec.ULN,
                        aec.NationalInsuranceNumber,
                        aec.UKPRN,
                        aec.ApprenticeshipId,
                        aec.AccountId,
                        aec.MinDate,
                        aec.MaxDate,
                        aec.CheckType,
                        aec.IsEmployed,
                        aec.LastUpdated,
                        aec.CreatedDate,
                        aec.HasBeenChecked)).ToList();
                }
                else
                {
                    logger.LogInformation($"{thisMethodName}: Database query returned [0] apprentices requiring employment check.");
                    apprenticeEmploymentCheckModels = new List<ApprenticeEmploymentCheckModel>(); // return an empty list rather than null
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the EmploymentCheckLastGetId failed - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticeEmploymentCheckModels;
        }

        private async Task<long> SaveTheEmploymentCheckLastGetId(
            ILogger logger,
            SqlConnection sqlConnection,
            long employmentCheckLastGetId)
        {
            var thisMethodName = $"{ThisClassName}.SaveTheEmploymentCheckLastGetId()";

            long updatedEmploymentCheckLastGetId = 0;
            try
            {
                // On first use the control table will be empty and will need an insert rather than an update
                var rowCountResult = await sqlConnection.QueryAsync<long>(
                    sql: "SELECT COUNT(*) FROM [dbo].[EmploymentChecksControlTable] WITH (NOLOCK)",
                    commandType: CommandType.Text);

                var rowCount = rowCountResult?.FirstOrDefault() ?? 0;

                if (rowCount > 0)
                {
                    // Update the existing row(s)
                    var parameters = new DynamicParameters();
                    parameters.Add("@employmentCheckLastGetId", employmentCheckLastGetId);

                    // Update the highest id of the batch as the starting point for the next batch
                    await sqlConnection.ExecuteAsync(
                       sql: "UPDATE [dbo].[EmploymentChecksControlTable] SET EmploymentCheckLastGetId = @employmentCheckLastGetId",
                       param: parameters,
                       commandType: CommandType.Text);
                }
                else
                {
                    // insert a row
                    // For the first call we set the employmentCheckLastGetId to zero to ensure we pick up the first batch
                    employmentCheckLastGetId = 0;

                    var parameters = new DynamicParameters();
                    parameters.Add("@employmentCheckLastGetId", employmentCheckLastGetId);

                    await sqlConnection.ExecuteAsync(
                       sql: "  INSERT [dbo].[EmploymentChecksControlTable] (RowId, EmploymentCheckLastGetId) VALUES (1, @employmentCheckLastGetId)",
                       param: parameters,
                       commandType: CommandType.Text);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the EmploymentCheckLastGetId failed - {ex.Message}. {ex.StackTrace}");
            }

            // Check the saved value matches the value we saved
            updatedEmploymentCheckLastGetId = await GetEmploymentCheckLastGetId(logger, sqlConnection);

            if (updatedEmploymentCheckLastGetId != employmentCheckLastGetId)
            {
                logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The updatedEmploymentCheckLastGetId [{updatedEmploymentCheckLastGetId}] value return from SaveTheEmploymentCheckLastGetId() does not match the value saved [{employmentCheckLastGetId}].");
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
        public abstract Task EnqueueApprenticeEmploymentCheckMessages_Service(ApprenticeRelatedData apprenticeEmploymentData);

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
            ApprenticeRelatedData apprenticeEmploymentData)
        {
            var thisMethodName = $"{ThisClassName}.EnqueueApprenticeEmploymentCheckMessages()";

            try
            {
                if (apprenticeEmploymentData != null)
                {
                    var apprenticeEmploymentCheckMessages = await CreateApprenticeEmploymentCheckMessages(logger, apprenticeEmploymentData);

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
        public abstract Task<ApprenticeEmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Service();

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<ApprenticeEmploymentCheckMessageModel></returns>
        public virtual async Task<ApprenticeEmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Base(
            ILogger logger,
            string connectionString,
            int batchSize,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.DequeueApprenticeEmploymentCheckMessage_Base()";

            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel = null;
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
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The checkModel value returned from the call to GetApprenticeEmploymentCheckMessage_Base() is null.");
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
        public virtual async Task<ApprenticeEmploymentCheckMessageModel> GetApprenticeEmploymentCheckMessage_Base(
              ILogger logger,
              string connectionString,
              string azureResource,
              int batchSize,
              AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticeEmploymentCheckMessage_Base()";

            ApprenticeEmploymentCheckMessageModel model = null;

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
                                           "FROM [dbo].[ApprenticeEmploymentCheckMessageQueue] " +
                                           "ORDER BY MessageId";

                        model = (await sqlConnection.QueryAsync<ApprenticeEmploymentCheckMessageModel>(sql, commandType: CommandType.Text)).FirstOrDefault();

                        if (model == null)
                        {
                            logger.LogInformation(
                                $"{thisMethodName}: {ErrorMessagePrefix} The checkModel returned from the LINQ statement employmentCheckMessageModels.FirstOrDefault() is null.");
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
                logger.LogError($"{thisMethodName} {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return model;
        }

        public abstract Task SaveEmploymentCheckResult_Service(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel);

        public virtual async Task SaveEmploymentCheckResult_Base(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            ApprenticeEmploymentCheckMessageModel checkModel)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckResult_Base()";

            try
            {
                if (checkModel != null)
                {
                    var sqlConnection = await CreateSqlConnection(logger, connectionString, azureResource, azureServiceTokenProvider);
                    if (sqlConnection == null)
                    {
                        logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} The sqlConnection value returned from CreateSqlConnection() is null.");
                    }
                    else
                    {
                        await using (sqlConnection)
                        {
                            await sqlConnection.OpenAsync();
                            var transaction = sqlConnection.BeginTransaction();
                            var parameters = new DynamicParameters();

                            parameters.Add("id", checkModel.EmploymentCheckId, DbType.Int64);
                            parameters.Add("messageId", checkModel.MessageId, DbType.Guid);
                            parameters.Add("isEmployed", checkModel.IsEmployed, DbType.Boolean);
                            parameters.Add("lastUpdated", checkModel.EmploymentCheckedDateTime, DbType.DateTime);
                            parameters.Add("returnCode", checkModel.ReturnCode, DbType.String);
                            parameters.Add("returnMessage", checkModel.ReturnMessage, DbType.String);
                            parameters.Add("hasBeenChecked", true);


                            logger.LogInformation($"{thisMethodName}: Updating row for ULN: {checkModel.Uln}.");

                            try
                            {
                                await sqlConnection.ExecuteAsync(
                                    "UPDATE [dbo].[EmploymentChecks] SET " +
                                    "IsEmployed = @isEmployed," +
                                    "LastUpdated = @lastUpdated," +
                                    "HasBeenChecked = @hasBeenChecked," +
                                    "ReturnCode = @returnCode," +
                                    "ReturnMessage = @returnMessage" +
                                    " WHERE Id = @id" +
                                    " AND isEmployed <> 1",
                                    parameters,
                                    commandType: CommandType.Text,
                                    transaction: transaction);

                                // -------------------------------------------------------------------
                                // Archive a copy of the employment check message for auditing
                                // -------------------------------------------------------------------
                                var checkHistoryModel = new ApprenticeEmploymentCheckMessageHistoryModel(checkModel);

                                parameters = new DynamicParameters();
                                parameters.Add("@messageHistoryId", checkHistoryModel.MessageHistoryId = Guid.NewGuid(), DbType.Guid);
                                parameters.Add("@messageHistoryCreatedDateTime", checkHistoryModel.MessageHistoryCreatedDateTime, DbType.DateTime);
                                parameters.Add("@messageId", checkHistoryModel.MessageId, DbType.Guid);
                                parameters.Add("@messageCreatedDateTime", checkHistoryModel.MessageCreatedDateTime, DbType.DateTime);
                                parameters.Add("@employmentCheckId", checkHistoryModel.EmploymentCheckId, DbType.Int64);
                                parameters.Add("@uln", checkHistoryModel.Uln, DbType.Int64);
                                parameters.Add("@nationalInsuranceNumber", checkHistoryModel.NationalInsuranceNumber, DbType.String);
                                parameters.Add("@payeScheme", checkHistoryModel.PayeScheme, DbType.String);
                                parameters.Add("@startDateTime", checkHistoryModel.StartDateTime, DbType.DateTime);
                                parameters.Add("@endDateTime", checkHistoryModel.EndDateTime, DbType.DateTime);
                                parameters.Add("@employmentCheckedDateTime", checkHistoryModel.EmploymentCheckedDateTime, DbType.DateTime);
                                parameters.Add("@isEmployed", checkHistoryModel.IsEmployed, DbType.Boolean);
                                parameters.Add("@returnCode", checkHistoryModel.ReturnCode, DbType.String);
                                parameters.Add("@returnMessage", checkHistoryModel.ReturnMessage, DbType.String);

                                await sqlConnection.ExecuteAsync(
                                    "INSERT [dbo].[ApprenticeEmploymentCheckMessageQueueHistory] " +
                                    "( " +
                                    " [MessageHistoryId] " +
                                    ",[MessageHistoryCreatedDateTime] " +
                                    ",[MessageId] " +
                                    ",[MessageCreatedDateTime] " +
                                    ",[EmploymentCheckId] " +
                                    ",[Uln] " +
                                    ",[NationalInsuranceNumber] " +
                                    ",[PayeScheme] " +
                                    ",[StartDateTime] " +
                                    ",[EndDateTime] " +
                                    ",[EmploymentCheckedDateTime] " +
                                    ",[IsEmployed] " +
                                    ",[ReturnCode] " +
                                    ",[ReturnMessage] " +
                                    ") " +
                                    "VALUES (" +
                                    "@messageHistoryId" +
                                    ",@messageHistoryCreatedDateTime" +
                                    ",@messageId " +
                                    ",@messageCreatedDateTime " +
                                    ",@employmentCheckId " +
                                    ",@uln " +
                                    ",@nationalInsuranceNumber " +
                                    ",@payeScheme " +
                                    ",@startDateTime " +
                                    ",@endDateTime" +
                                    ",@EmploymentCheckedDateTime " +
                                    ",@IsEmployed " +
                                    ",@ReturnCode " +
                                    ",@ReturnMessage)",
                                    parameters,
                                    commandType: CommandType.Text,
                                    transaction: transaction);

                                parameters = new DynamicParameters();
                                parameters.Add("messageId", checkModel.MessageId, DbType.Guid);

                                await sqlConnection.ExecuteAsync(
                                    "DELETE [dbo].[ApprenticeEmploymentCheckMessageQueue] WHERE MessageId = @messageId",
                                    parameters,
                                    commandType: CommandType.Text,
                                    transaction: transaction);

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
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        public virtual async Task<IList<ApprenticeEmploymentCheckMessageModel>> CreateApprenticeEmploymentCheckMessages(ILogger logger, ApprenticeRelatedData apprenticeEmploymentData)
        {
            var thisMethodName = $"EmploymentCheckService.CreateApprenticeEmploymentCheckMessages()";

            var apprenticeEmploymentCheckMessages = new List<ApprenticeEmploymentCheckMessageModel>();
            try
            {
                if (apprenticeEmploymentData is {ApprenticeEmploymentChecks: { }})
                {
                    // Create an ApprenticeEmploymentCheckMessage for each combination of Uln, National Insurance Number and PayeSchemes.
                    // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln and National Insurance Number
                    // Note: Once we get a 'hit' for a given Nino, PayeScheme, StartDate, EndDate from the HMRC API we could discard the remaining messages without calling the API if necessary
                    // (at this point we don't know if we need to check all the payeschemes for a given apprentice just to ensure there's no match on multiple schemes)
                    foreach (var apprentice in apprenticeEmploymentData.ApprenticeEmploymentChecks)
                    {

                        // Get the National Insurance Number for this apprentice
                        var apprenticeNiNumber = apprenticeEmploymentData.ApprenticeNiNumbers.FirstOrDefault(ninumber => ninumber.ULN == apprentice.ULN);
                        if (apprenticeNiNumber == null)
                        {
                            logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} {nameof(apprenticeNiNumber)} is null.");
                        }
                        var nationalInsuranceNumber = apprenticeNiNumber?.NationalInsuranceNumber;
                       
                        // Get the employer paye schemes for this apprentices
                        var employerPayeSchemes = apprenticeEmploymentData.EmployerPayeSchemes.FirstOrDefault(ps => ps.EmployerAccountId == apprentice.AccountId);
                        if (employerPayeSchemes == null)
                        {
                            logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} {nameof(employerPayeSchemes)} is null.");
                            employerPayeSchemes = new EmployerPayeSchemes(apprentice.AccountId, new List<string> {null});
                        }

                        // Create the individual message combinations for each paye scheme
                        foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                        {
                            var message = new ApprenticeEmploymentCheckMessageModel
                            {
                                EmploymentCheckId = apprentice.Id,
                                Uln = apprentice.ULN,
                                NationalInsuranceNumber = nationalInsuranceNumber,
                                StartDateTime = apprentice.MinDate,
                                EndDateTime = apprentice.MaxDate,
                                PayeScheme = payeScheme
                            };

                            apprenticeEmploymentCheckMessages.Add(message);
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

            return await Task.FromResult(apprenticeEmploymentCheckMessages);
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
            IList<ApprenticeEmploymentCheckMessageModel> apprenticeEmploymentCheckMessages)
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
                            logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Creation of SQL Connection for the Employment Check Database failed.");
                        }
                    }
                }
                else
                {
                    logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No apprentice employment check queue messages to store.");
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
        /// <param name="apprenticeEmploymentCheckMessage"></param>
        /// <returns>Task</returns>
        public virtual async Task StoreApprenticeEmploymentCheckMessage(
            ILogger logger,
            string connectionString,
            SqlConnection sqlConnection,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessage)
        {
            var thisMethodName = $"{ThisClassName}.StoreApprenticeEmploymentCheckMessage()";

            try
            {
                if (apprenticeEmploymentCheckMessage != null)
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


                    var parameters = new DynamicParameters();
                    parameters.Add("@messageId", apprenticeEmploymentCheckMessage.MessageId = Guid.NewGuid(), DbType.Guid);
                    parameters.Add("@messageCreatedDateTime", apprenticeEmploymentCheckMessage.MessageCreatedDateTime = DateTime.Now, DbType.DateTime);
                    parameters.Add("@employmentCheckId", apprenticeEmploymentCheckMessage.EmploymentCheckId, DbType.Int64);
                    parameters.Add("@uln", apprenticeEmploymentCheckMessage.Uln, DbType.Int64);
                    parameters.Add("@nationalInsuranceNumber", apprenticeEmploymentCheckMessage.NationalInsuranceNumber, DbType.String);
                    parameters.Add("@payeScheme", apprenticeEmploymentCheckMessage.PayeScheme, DbType.String);
                    parameters.Add("@startDateTime", apprenticeEmploymentCheckMessage.StartDateTime, DbType.DateTime);
                    parameters.Add("@endDateTime", apprenticeEmploymentCheckMessage.EndDateTime, DbType.DateTime);
                    parameters.Add("@employmentCheckedDateTime", apprenticeEmploymentCheckMessage.EmploymentCheckedDateTime, DbType.DateTime);
                    parameters.Add("@isEmployed", apprenticeEmploymentCheckMessage.IsEmployed ?? false, DbType.Boolean);

                    const string sql = "INSERT INTO [dbo].[ApprenticeEmploymentCheckMessageQueue] " +
                                       "(MessageId, " +
                                       "MessageCreatedDateTime, " +
                                       "EmploymentCheckId, " +
                                       "Uln, " +
                                       "NationalInsuranceNumber, " +
                                       "PayeScheme, " +
                                       "StartDateTime, " +
                                       "EndDateTime) " +
                                       "SELECT " +
                                       "@messageId, " +
                                       "@messageCreatedDateTime, " +
                                       "@employmentCheckId, " +
                                       "@uln, " +
                                       "@nationalInsuranceNumber, " +
                                       "@payeScheme, " +
                                       "@startDateTime, " +
                                       "@endDateTime " +
                                       "WHERE NOT EXISTS " +
                                       "(SELECT 1 FROM [dbo].[ApprenticeEmploymentCheckMessageQueue] " +
                                       "WHERE EmploymentCheckId = @employmentCheckId " +
                                       "AND PayeScheme = @payeScheme" +
                                       ");";

                    await sqlConnection.ExecuteAsync(sql, commandType: CommandType.Text, param: parameters);
                }
                else
                {
                    logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No apprentice employment check queue mesaages to store.");
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
                if (string.IsNullOrEmpty(connectionString))
                {
                    logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Missing SQL connection string for the Employment Check Database.");
                    return null;
                }

                sqlConnection = new SqlConnection(connectionString);

                if (azureServiceTokenProvider != null) // no token required for local db
                {
                    if (string.IsNullOrEmpty(azureResource))
                    {
                        logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Missing AzureResource string for the Employment Check Database.");
                        return null;
                    }

                    sqlConnection.AccessToken = await azureServiceTokenProvider.GetAccessTokenAsync(azureResource);
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
                            var parameters = new DynamicParameters();
                            parameters.Add("@uln", 1000000000 + i, DbType.Int64);
                            parameters.Add("@ukprn", 10000000 + i, DbType.Int64);
                            parameters.Add("@apprenticeshipId", i, DbType.Int64);
                            parameters.Add("@accountId", i, DbType.Int64);
                            parameters.Add("@minDateSeed", input.StartDateTime, DbType.DateTime);
                            parameters.Add("@maxDateSeed", input.EndDateTime, DbType.DateTime);
                            parameters.Add("@checkTypeSeed", "StartDate+60", DbType.String);
                            parameters.Add("@isEmployedSeed", false, DbType.Boolean);
                            parameters.Add("@createdDateSeed", DateTime.Now, DbType.DateTime);
                            parameters.Add("@hasBeenCheckedSeed", false, DbType.Boolean);

                            await connection.ExecuteAsync(
                                "INSERT [dbo].[EmploymentChecks] (ULN, UKPRN, ApprenticeshipId, AccountId, MinDate, MaxDate, CheckType, IsEmployed, CreatedDate, HasBeenChecked) VALUES (@uln, @ukprn, @apprenticeshipId, @accountId, @minDateSeed, @maxDateSeed, @checkTypeSeed, @isEmployedSeed, @createdDateSeed, @hasBeenCheckedSeed)",
                                commandType: CommandType.Text,
                                param: parameters);
                        }
                    }
                    else
                    {
                        logger.LogInformation(
                            $"\n\n{DateTime.UtcNow} {thisMethodName}: *** ERROR ***: Creation of SQL Connection for the Employment Check Database failed.");
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