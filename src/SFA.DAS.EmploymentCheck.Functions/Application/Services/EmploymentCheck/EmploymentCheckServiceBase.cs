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
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public abstract class EmploymentCheckServiceBase
        : IEmploymentCheckService
    {
        private const string ThisClassName = "\n\nEmploymentCheckServiceBase";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        // The EmploymentCheckService and EmploymentCheckServiceStub can implement this method to either call the database code or the stub code as appropriate
        public abstract Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Service();

        /// <summary>
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<IList<ApprenticeEmploymentCheckModel>></returns>
        public virtual async Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecks_Base(
            ILogger logger,
            string connectionString,
            string azureResource,
            int batchSize,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticeEmploymentChecks_Base()";

            IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentChecks = null;
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
                        await using (sqlConnection)
                        {

                            var parameters = new DynamicParameters();
                            parameters.Add("@batchSize", batchSize);

                            await sqlConnection.OpenAsync();
                            var apprenticeEmploymentChecksResult = await sqlConnection.QueryAsync<ApprenticeEmploymentCheckModel>(
                                sql: "SELECT TOP (@batchSize) Id, ULN, UKPRN, ApprenticeshipId, AccountId, NationalInsuranceNumber, MinDate, MaxDate, CheckType, IsEmployed, LastUpdated, CreatedDate, HasBeenChecked FROM [dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate",
                                param: parameters,
                                commandType: CommandType.Text);

                            if (apprenticeEmploymentChecksResult != null &&
                                apprenticeEmploymentChecksResult.Any())
                            {
                                logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: Database query returned [{apprenticeEmploymentChecksResult.Count()}] apprentices requiring employment check.");

                                apprenticeEmploymentChecks = apprenticeEmploymentChecksResult.Select(aec => new ApprenticeEmploymentCheckModel(
                                    aec.Id,
                                    aec.ULN,
                                    aec.UKPRN,
                                    aec.ApprenticeshipId,
                                    aec.AccountId,
                                    aec.NationalInsuranceNumber,
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
                                logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: Database query returned [0] apprentices requiring employment check.");
                                apprenticeEmploymentChecks = new List<ApprenticeEmploymentCheckModel>(); // return an empty list rather than null
                            }
                        }
                    }
                    else
                    {
                        logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The sqlConnection value returned from CreateSqlConnection() is null.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticeEmploymentChecks;
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

                    }
                }
                else
                {
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentData input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
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
                    logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModel value returned from the call to GetApprenticeEmploymentCheckMessage_Base() is null.");
                }

            }
            catch (Exception ex)
            {
                logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
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

            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel = null;
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

                        var parameters = new DynamicParameters();
                        parameters.Add("@batchSize", batchSize);

                        await sqlConnection.OpenAsync();
                        // TODO: The expectation is that there is only one instance of this code getting the message from the database.
                        //       If there are going to be multiple instances of the code pulling the messages off the message queue
                        //       then we will need to make the select/delete of the message transational to lock the row so that
                        //       other instances aren't pulling the same message
                        var employmentCheckMessageModels = await sqlConnection.QueryAsync<ApprenticeEmploymentCheckMessageModel>(
                            sql: "SELECT TOP (1) MessageId, MessageCreatedDateTime, Uln, NationalInsuranceNumber, PayeScheme, StartDateTime, EndDateTime, EmploymentCheckedDateTime, IsEmployed, ReturnCode, ReturnMessage FROM [dbo].[ApprenticeEmploymentCheckMessageQueue] WHERE EmploymentCheckedDateTime IS NOT NULL ORDER BY MessageCreatedDateTime",
                            param: parameters,
                            commandType: CommandType.Text);

                        if (employmentCheckMessageModels != null &&
                            employmentCheckMessageModels.Any())
                        {
                            logger.LogInformation($"{thisMethodName}: Database query returned {employmentCheckMessageModels.Count()} apprentices.");
                            apprenticeEmploymentCheckMessageModel = (ApprenticeEmploymentCheckMessageModel)employmentCheckMessageModels.Select(m => new ApprenticeEmploymentCheckMessageModel(
                                m.MessageId,
                                m.MessageCreatedDateTime,
                                m.EmploymentCheckId,
                                m.Uln,
                                m.NationalInsuranceNumber,
                                m.PayeScheme,
                                m.StartDateTime,
                                m.EndDateTime,
                                m.EmploymentCheckedDateTime,
                                m.IsEmployed,
                                m.ReturnCode,
                                m.ReturnMessage));
                        }
                        else
                        {
                            logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessageModels value returned from the database query is null.");
                        }
                    }
                    else
                    {
                        logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The sqlConnection value returned from CreateSqlConnection() is null.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticeEmploymentCheckMessageModel;
        }

        public abstract Task SaveEmploymentCheckResult_Service(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel);

        public virtual async Task SaveEmploymentCheckResult_Base(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider,
            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckResult_Base()";

            try
            {
                if (apprenticeEmploymentCheckMessageModel != null)
                {
                    await using (var sqlConnection = await CreateSqlConnection(
                        logger,
                        connectionString,
                        azureResource,
                        azureServiceTokenProvider))
                    {
                        if (sqlConnection != null)
                        {
                            await using (sqlConnection)
                            {
                                var parameters = new DynamicParameters();

                                parameters.Add("id", apprenticeEmploymentCheckMessageModel.EmploymentCheckId, DbType.Int64);
                                parameters.Add("isEmployed", apprenticeEmploymentCheckMessageModel.IsEmployed, DbType.Boolean);
                                parameters.Add("hasBeenChecked", true);

                                await sqlConnection.OpenAsync();

                                logger.LogInformation($"{thisMethodName}: Updating row for ULN: {apprenticeEmploymentCheckMessageModel.Uln}.");
                                await sqlConnection.ExecuteAsync(
                                    "UPDATE [dbo].[EmploymentChecks] SET IsEmployed = @isEmployed, LastUpdated = GETDATE(), HasBeenChecked = @hasBeenChecked WHERE Id = @id",
                                        parameters,
                                        commandType: CommandType.Text);
                            }
                        }
                        else
                        {
                            logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The sqlConnection value returned from CreateSqlConnection() is null.");
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
                logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        public virtual async Task<IList<ApprenticeEmploymentCheckMessageModel>> CreateApprenticeEmploymentCheckMessages(ILogger logger, ApprenticeRelatedData apprenticeEmploymentData)
        {
            var thisMethodName = $"EmploymentCheckService.CreateApprenticeEmploymentCheckMessages()";

            var apprenticeEmploymentCheckMessages = new List<ApprenticeEmploymentCheckMessageModel>();
            try
            {
                if (apprenticeEmploymentData != null &&
                    apprenticeEmploymentData.ApprenticeEmploymentChecks != null &&
                    apprenticeEmploymentData.ApprenticeNiNumbers != null &&
                    apprenticeEmploymentData.EmployerPayeSchemes != null)
                {
                    // Create an ApprenticeEmploymentCheckMessage for each combination of Uln, National Insurance Number and PayeSchemes.
                    // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln and National Insurance Number
                    // Note: Once we get a 'hit' for a given Nino, PayeScheme, StartDate, EndDate from the HMRC API we could discard the remaining messages without calling the API if necessary
                    // (at this point we don't know if we need to check all the payeschemes for a given apprentice just to ensure there's no match on multiple schemes)
                    foreach (var apprentice in apprenticeEmploymentData.ApprenticeEmploymentChecks)
                    {
                        var apprenticeEmploymentCheckMessage = new ApprenticeEmploymentCheckMessageModel();

                        // Get the employer paye schemes for this apprentices
                        var employerPayeSchemes = apprenticeEmploymentData.EmployerPayeSchemes.Where(ps => ps.EmployerAccountId == apprentice.AccountId).FirstOrDefault();

                        // Create the individual message combinations for each paye scheme
                        foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                        {
                            apprenticeEmploymentCheckMessage.EmploymentCheckId = apprentice.Id;
                            apprenticeEmploymentCheckMessage.Uln = apprentice.ULN;
                            apprenticeEmploymentCheckMessage.NationalInsuranceNumber = apprenticeEmploymentData.ApprenticeNiNumbers.Where(ninumber => ninumber.ULN == apprentice.ULN).FirstOrDefault().NationalInsuranceNumber;
                            apprenticeEmploymentCheckMessage.StartDateTime = apprentice.MinDate;
                            apprenticeEmploymentCheckMessage.EndDateTime = apprentice.MinDate;
                            apprenticeEmploymentCheckMessage.PayeScheme = payeScheme;

                            apprenticeEmploymentCheckMessages.Add(apprenticeEmploymentCheckMessage);
                        }
                    }
                }
                else
                {
                    logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: The Apprentice Employment Data supplied to create Apprentice Employment Check Messages is incomplete.");
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
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
                    await using (var connection = await CreateSqlConnection(
                        logger,
                        connectionString,
                        azureResource,
                        azureServiceTokenProvider))
                    {
                        if (connection != null)
                        {
                            await connection.OpenAsync();

                            foreach (var apprenticeEmploymentCheckMessage in apprenticeEmploymentCheckMessages)
                            {
                                var parameters = new DynamicParameters();
                                parameters.Add("@messageId", apprenticeEmploymentCheckMessage.MessageId = Guid.NewGuid(), DbType.Guid);
                                parameters.Add("@messageCreatedDateTime", apprenticeEmploymentCheckMessage.MessageCreatedDateTime = DateTime.Now, DbType.DateTime);
                                parameters.Add("@employmentCheckId", apprenticeEmploymentCheckMessage.EmploymentCheckId, DbType.Int64);
                                parameters.Add("@uln", apprenticeEmploymentCheckMessage.Uln, DbType.Int64);
                                parameters.Add("@nationalInsuranceNumber", apprenticeEmploymentCheckMessage.NationalInsuranceNumber, DbType.String);
                                parameters.Add("@payeScheme", apprenticeEmploymentCheckMessage.PayeScheme, DbType.String);
                                parameters.Add("@startDateTime", apprenticeEmploymentCheckMessage.StartDateTime, DbType.Date);
                                parameters.Add("@endDateTime", apprenticeEmploymentCheckMessage.EndDateTime, DbType.DateTime);
                                parameters.Add("@employmentCheckedDateTime", apprenticeEmploymentCheckMessage.EmploymentCheckedDateTime, DbType.DateTime);
                                parameters.Add("@isEmployed", apprenticeEmploymentCheckMessage.IsEmployed ?? false, DbType.Boolean);

                                await connection.ExecuteAsync(
                                    sql: "INSERT [dbo].[ApprenticeEmploymentCheckMessageQueue] (MessageId, MessageCreatedDateTime, EmploymentCheckId, Uln, NationalInsuranceNumber, PayeScheme, StartDateTime, EndDateTime) VALUES (@messageId, @messageCreatedDateTime, @employmentCheckId, @uln, @nationalInsuranceNumber, @payeScheme, @startDateTime, @endDateTime)",
                                    commandType: CommandType.Text,
                                    param: parameters);
                            }
                        }
                        else
                        {
                            logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: {ErrorMessagePrefix} Creation of SQL Connection for the Employment Check Databasse failed.");
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
                logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
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
                        }
                        else
                        {
                            logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: *** ERROR ***: Creation of SQL Connection for the Employment Check Databasse failed.");
                        }
                    }
                    else
                    {
                        logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: Missing AzureResource string for the Employment Check Databasse.");
                    }
                }
                else
                {
                    logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: Missing SQL connecton string for the Employment Check Databasse.");
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
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

                        var ulnSeed = 1000000000;
                        var ukprnSeed = 10000000;
                        var nationalInsuranceNumberSeed = 100000000;
                        DateTime minDateSeed = DateTime.Now.Date;
                        DateTime maxDateSeed = DateTime.Now.AddDays(60);
                        string checkTypeSeed = "StartDate+60";
                        bool isEmployedSeed = false;
                        DateTime createdDateSeed = DateTime.Now;
                        bool hasBeenCheckedSeed = false;

                        for (int i = 1; i <= 10; i++)
                        {
                            var parameters = new DynamicParameters();
                            parameters.Add("@uln", ulnSeed + i, DbType.Int64);
                            parameters.Add("@ukprn", ukprnSeed + i, DbType.Int64);
                            parameters.Add("@apprenticeshipId", i, DbType.Int64);
                            parameters.Add("@accountId", i, DbType.Int64);
                            //parameters.Add("@nationalInsuranceNumber", (nationalInsuranceNumberSeed + i).ToString(), DbType.String);
                            parameters.Add("@minDateSeed", minDateSeed, DbType.DateTime);
                            parameters.Add("@maxDateSeed", maxDateSeed, DbType.DateTime);
                            parameters.Add("@checkTypeSeed", checkTypeSeed, DbType.String);
                            parameters.Add("@isEmployedSeed", isEmployedSeed, DbType.Boolean);
                            parameters.Add("@createdDateSeed", createdDateSeed, DbType.DateTime);
                            parameters.Add("@hasBeenCheckedSeed", hasBeenCheckedSeed, DbType.Boolean);

                            await connection.ExecuteAsync(
                                //sql: "INSERT [dbo].[EmploymentChecks] (ULN, UKPRN, ApprenticeshipId, AccountId, NationalInsuranceNumber, MinDate, MaxDate, CheckType, IsEmployed, CreatedDate, HasBeenChecked) VALUES (@uln, @ukprn, @apprenticeshipId, @accountId, @nationalInsuranceNumber, @minDateSeed, @maxDateSeed, @checkTypeSeed, @isEmployedSeed, @createdDateSeed, @hasBeenCheckedSeed)",
                                sql: "INSERT [dbo].[EmploymentChecks] (ULN, UKPRN, ApprenticeshipId, AccountId, MinDate, MaxDate, CheckType, IsEmployed, CreatedDate, HasBeenChecked) VALUES (@uln, @ukprn, @apprenticeshipId, @accountId, @minDateSeed, @maxDateSeed, @checkTypeSeed, @isEmployedSeed, @createdDateSeed, @hasBeenCheckedSeed)",
                                commandType: CommandType.Text,
                                param: parameters);
                        }
                    }
                    else
                    {
                        logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: *** ERROR ***: Creation of SQL Connection for the Employment Check Databasse failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}