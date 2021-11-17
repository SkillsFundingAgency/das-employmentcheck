using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : EmploymentCheckServiceBase
    {
        private const string ThisClassName = "\n\nEmploymentCheckService";

        private EmploymentCheckDbConfiguration _employmentCheckDbConfiguration;
        private const string AzureResource = "https://database.windows.net/";
        private readonly string _connectionString;
        private readonly int _batchSize;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private ILogger<IEmploymentCheckService> _logger;

        /// <summary>
        /// ------------------------------------------------------------------------------------
        /// The production implementation of the methods definined in the EmploymentCheckService
        /// ------------------------------------------------------------------------------------
        /// </summary>
        /// <param name="applicationSettings"></param>
        /// <param name="employmentCheckDbConfiguration"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="logger"></param>
        public EmploymentCheckService(
            ApplicationSettings applicationSettings,                            // TODO: Replace this generic application setting
            EmploymentCheckDbConfiguration employmentCheckDbConfiguration,      // TODO: With this specific employment check database configuration
            AzureServiceTokenProvider azureServiceTokenProvider,
            ILogger<IEmploymentCheckService> logger)
        {
            _connectionString = applicationSettings.DbConnectionString;
            _employmentCheckDbConfiguration = employmentCheckDbConfiguration;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _batchSize = applicationSettings.BatchSize;
            _logger = logger;
        }

        /// <summary>
        /// --------------------------------------------------------------------------------------------------
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database
        /// --------------------------------------------------------------------------------------------------
        /// </summary>
        /// <returns>Task<IList<ApprenticeEmploymentCheckModel>></returns>
        public async override Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Service()
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticeEmploymentChecks()";

            IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentChecks = null;
            try
            {
                apprenticeEmploymentChecks = await GetApprenticeEmploymentChecks_Base(
                      _logger,
                      _connectionString,
                      AzureResource,
                      _batchSize,
                      _azureServiceTokenProvider);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticeEmploymentChecks;
        }

        /// <summary>
        /// ---------------------------------------------------------------------------------------------------------------------------------
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue
        /// ---------------------------------------------------------------------------------------------------------------------------------
        /// </summary>
        /// <param name="apprenticeEmploymentData"></param>
        /// <returns>Task</returns>
        public async override Task EnqueueApprenticeEmploymentCheckMessages_Service(ApprenticeRelatedData apprenticeEmploymentData)
        {
            // TODO: Add implementation for using Azure SqlDatabase
            var thisMethodName = $"{ThisClassName}.EnqueueApprenticeEmploymentCheckMessages_Service()";

            try
            {
                if (apprenticeEmploymentData != null)
                {
                    await EnqueueApprenticeEmploymentCheckMessages_Service(
                        _logger,
                        _connectionString,
                        AzureResource,
                        _azureServiceTokenProvider,
                        apprenticeEmploymentData);
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentChecks return value from the base class service call to GetApprenticeEmploymentCheck returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        /// <summary>
        /// --------------------------------------------------------------------------------------------------------
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API
        /// --------------------------------------------------------------------------------------------------------
        /// </summary>
        /// <returns>Task<ApprenticeEmploymentCheckMessageModel></returns>
        public async override Task<ApprenticeEmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Service()
        {
            // TODO: Add implementation for using Azure SqlDatabase
            var thisMethodName = $"{ThisClassName}.DequeueApprenticeEmploymentCheckMessage_Service()";

            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel = null;
            try
            {
                apprenticeEmploymentCheckMessageModel = await DequeueApprenticeEmploymentCheckMessage_Base(
                    _logger,
                    _connectionString,
                    _batchSize,
                    AzureResource,
                    _azureServiceTokenProvider);

                if(apprenticeEmploymentCheckMessageModel == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModel value returned from the call to DequeueApprenticeEmploymentCheckMessage_Base() is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return apprenticeEmploymentCheckMessageModel;
        }

        //public async override Task SaveEmploymentCheckResults(IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentCheckModels)
        //{
        //    var thisMethodName = $"{ThisClassName}.EnqueueApprenticeEmploymentCheckMessages()";

        //    try
        //    {
        //        if (apprenticeEmploymentCheckModels != null &&
        //            apprenticeEmploymentCheckModels.Any())
        //        {
        //            await SaveEmploymentCheckResults(
        //                 _logger,
        //                 _connectionString,
        //                 AzureResource,
        //                 _azureServiceTokenProvider,
        //                 apprenticeEmploymentCheckModels);
        //        }
        //        else
        //        {
        //            _logger.LogInformation($"{thisMethodName}: No apprentice employment check queue messaages to store.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }
        //}

        //public async override Task<int> SaveEmploymentCheckResult(long id, bool result)
        //{
        //    await using (var connection = await CreateSqlConnection(
        //                _logger,
        //                _connectionString,
        //                AzureResource,
        //                _azureServiceTokenProvider))
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@id", id);
        //        parameters.Add("@isEmployed", result);

        //        await connection.OpenAsync();
        //        await connection.ExecuteAsync(
        //            sql: "UPDATE dbo.EmploymentChecks SET IsEmployed = @isEmployed, LastUpdated = GETDATE(), HasBeenChecked = 1 WHERE Id = @id",
        //            commandType: CommandType.Text,
        //            param: parameters);
        //    }

        //    // TODO: Return the result of the ExecuteAsync() call
        //    return await Task.FromResult(0);
        //}

        //public override Task<int> SaveEmploymentCheckResult(long id, long uln, bool result)
        //{
        //    //Required for stub
        //    //TO DO Remove when stub is removed
        //    throw new NotImplementedException();
        //}


        public async override Task SeedEmploymentCheckApprenticeDatabaseTableTestData()
        {
            await SeedEmploymentCheckApprenticeDatabaseTableTestData(
                _logger,
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider);
        }

        public override Task SaveEmploymentCheckResult_Service(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            throw new NotImplementedException();
        }

        // ------------------------------------------------------------------------
        // The code below can be deleted once we're sure that it's no longer needed
        // ------------------------------------------------------------------------

        //public async override Task<IList<ApprenticeEmploymentCheckMessage>> DequeueApprenticeEmploymentCheckMessages(
        //    ILogger logger,
        //    string connectionString,
        //    int batchSize,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider)
        //{
        //    return await DequeueApprenticeEmploymentCheckMessages(
        //        _logger,
        //        _connectionString,
        //        _batchSize,
        //        AzureResource,
        //        _azureServiceTokenProvider);
        //}

        //public async override Task<int> EnqueueApprenticeEmploymentCheckMessages(ApprenticeEmploymentData apprenticeEmploymentData)
        //{
        //    var thisMethodName = $"EmploymentCheckService.EnqueueApprenticeEmploymentCheckMessages()";

        //    try
        //    {
        //        if (apprenticeEmploymentData != null)
        //        {
        //            var apprenticeEmploymentCheckMessages = await CreateApprenticeEmploymentCheckMessages(_logger, apprenticeEmploymentData);

        //            await using (var connection = await CreateConnection())
        //            {
        //                foreach (var queueMessage in apprenticeEmploymentCheckMessages)
        //                {
        //                    var parameters = new DynamicParameters();
        //                    parameters.Add("@messageId", queueMessage.MessageId = Guid.NewGuid(), DbType.Guid);
        //                    parameters.Add("@uln", queueMessage.Uln, DbType.Int64);
        //                    parameters.Add("@nationalInsuranceNumber", queueMessage.NationalInsuranceNumber, DbType.String);
        //                    parameters.Add("@payeScheme", queueMessage.PayeScheme, DbType.String);
        //                    parameters.Add("@startDateTime", queueMessage.StartDateTime, DbType.Date);
        //                    parameters.Add("@endDateTime", queueMessage.EndDateTime, DbType.DateTime);
        //                    parameters.Add("@employmentCheckedDateTime", queueMessage.EmploymentCheckedDateTime, DbType.DateTime);
        //                    parameters.Add("@isEmployed", queueMessage.IsEmployed ?? false, DbType.Boolean);

        //                    await connection.OpenAsync();
        //                    await connection.ExecuteAsync(
        //                        sql: "INSERT dbo.EmploymentChecks (MessageId, Uln, NationalInsuranceNumber, PayeScheme, StartDateTime, EndDateTime, EmploymentCheckedDateTime, IsEmployed) VALUES (@messageId, @uln, @nationalInsurenanceNumber, @startDateTime, @endDateTime, @isEmployed)",
        //                        commandType: CommandType.Text,
        //                        param: parameters);

        //                }
        //            }
        //        }
        //        else
        //        {
        //            _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No apprentice employment check queue messaages to store.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    // TODO: Return the result of the ExecuteAsync() call
        //    return await Task.FromResult(0);
        //}


        //    public async Task<IList<ApprenticeEmploymentCheckMessage>> CreateApprenticeEmploymentCheckMessages(ApprenticeEmploymentData apprenticeEmploymentData)
        //    {
        //        var thisMethodName = $"EmploymentCheckService.CreateApprenticeEmploymentCheckMessages()";

        //        var apprenticeEmploymentCheckMessages = new List<ApprenticeEmploymentCheckMessage>();
        //        try
        //        {
        //            if (apprenticeEmploymentData != null &&
        //                apprenticeEmploymentData.Apprentices != null &&
        //                apprenticeEmploymentData.ApprenticeNiNumbers != null &&
        //                apprenticeEmploymentData.EmployerPayeSchemes != null)
        //            {
        //                // Create an ApprenticeEmploymentCheckMessage for each combination of Uln, National Insurance Number and PayeSchemes.
        //                // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln and National Insurance Number
        //                // Note: Once we get a 'hit' for a given Nino, PayeScheme, StartDate, EndDate from the HMRC API we can discard the remaining messages without calling the API if necessary
        //                // (at this point we don't know if we need to check all the payeschemes for a given apprentice just to ensure there's no match on multiple schemes)
        //                foreach (var apprentice in apprenticeEmploymentData.Apprentices)
        //                {
        //                    var apprenticeEmploymentCheckMessage = new ApprenticeEmploymentCheckMessage();

        //                    // Get all the paye schemes for this apprentices employer
        //                    var payeSchemes = apprenticeEmploymentData.EmployerPayeSchemes.Where(ps => ps.EmployerAccountId == apprentice.AccountId).ToList();

        //                    // Create the individual message combinations for each paye scheme
        //                    foreach (var payeScheme in payeSchemes)
        //                    {
        //                        apprenticeEmploymentCheckMessage.Uln = apprentice.ULN;
        //                        apprenticeEmploymentCheckMessage.NationalInsuranceNumber = apprenticeEmploymentData.ApprenticeNiNumbers.Where(ninumber => ninumber.ULN == apprentice.ULN).FirstOrDefault().NationalInsuranceNumber;
        //                        apprenticeEmploymentCheckMessage.PayeScheme = payeScheme.PayeSchemes.ToString();
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: The Apprentice Employment Data supplied to create Apprentice Employment Check Messages is incomplete.");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
        //        }

        //        return await Task.FromResult(apprenticeEmploymentCheckMessages);
        //    }

        //public async override Task<IList<ApprenticeEmploymentCheck>> GetApprenticeEmploymentChecks()
        //{
        //    var thisMethodName = $"EmploymentCheckService.GetApprentices()";

        //    IList<Apprentice> apprentices = null;
        //    try
        //    {
        //        var sqlConnection = new SqlConnection(_connectionString);   // TODO: this needs to work for both the Azure Sql db and a local Sql Server db
        //        if (sqlConnection != null)
        //        {
        //            await using (sqlConnection)
        //            {

        //                var parameters = new DynamicParameters();
        //                parameters.Add("@batchSize", _batchSize);

        //                await sqlConnection.OpenAsync();
        //                var employmentCheckResults = await sqlConnection.QueryAsync<EmploymentCheckResult>(
        //                    sql: "SELECT TOP (@batchSize) * FROM [dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate",
        //                    param: parameters,
        //                    commandType: CommandType.Text);

        //                if (employmentCheckResults != null && employmentCheckResults.Any())
        //                {
        //                    Log.WriteLog(_logger, thisMethodName, $"Database query returned {employmentCheckResults.Count()} apprentices.");
        //                    apprentices = employmentCheckResults.Select(x => new Apprentice(
        //                        x.Id,
        //                        x.AccountId,
        //                        x.NationalInsuranceNumber,
        //                        x.ULN,
        //                        x.UKPRN,
        //                        x.ApprenticeshipId,
        //                        x.MinDate,
        //                        x.MaxDate)).ToList();
        //                }
        //                else
        //                {
        //                    Log.WriteLog(_logger, thisMethodName, $"Database query returned [0] apprentices.");
        //                    apprentices = new List<Apprentice>(); // return an empty list rather than null
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Log.WriteLog(_logger, thisMethodName, "*** ERROR ****: sqlConnection string is NULL, no apprentices requiring employment check were retrieved.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
        //    }

        //    return apprentices;
        //}
    }
}