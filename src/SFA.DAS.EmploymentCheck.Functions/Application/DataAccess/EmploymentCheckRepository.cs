using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.DataAccess
{
    public class EmploymentCheckRepository
    {
        private const string ThisClassName = "\n\nEmploymentCheckCacheRequestRepository";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private const string AzureResource = "https://database.windows.net/";

        private ILogger _logger;
        private readonly string _connectionString;
        private readonly string _azureResource;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckRepository(
            ILogger logger,
            string connectionString,
            string azureResource)
        {
            _logger = logger;
            _connectionString = connectionString;
            _azureResource = azureResource;
        }

        public async Task<int> Insert<T>(T entity)
            where T : class
        {
            int result = 0;
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
                            sqlConnection.Open();

                            result = await sqlConnection.InsertAsync(entity);
                        }
                    }
                }

            }

            return result;
        }

        public async Task<long> GetEmploymentCheckLastHighestBatchId(
            // note the sqlConnection and transaction are needed as this code is also called from within the Dequeing messge code
            SqlConnection sqlConnection,
            SqlTransaction transaction)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckLastHighestBatchId()";

            long employmentCheckLastHighestBatchId = 0;
            try
            {
                // Get the highest EmploymentCheckId from the message queue
                // if there are no messages in the message queue
                // Get the highest EmploymentCheckId from the message queue history
                // if there are no messages in the message queue history
                // use the default value of zero
                if (sqlConnection == null)
                {
                    var dbConnection = new DbConnection();
                    sqlConnection = await dbConnection.CreateSqlConnection(
                       _logger,
                       _connectionString,
                       AzureResource,
                       _azureServiceTokenProvider);
                }

                if (sqlConnection != null)
                {
                    _logger.LogInformation($"{thisMethodName}: Getting the EmploymentCheckLastHighestBatchId");

                    using (sqlConnection)
                    {
                        await sqlConnection.OpenAsync();

                        employmentCheckLastHighestBatchId =
                        (
                            await sqlConnection.QueryAsync<long>(
                                sql:
                                "SELECT " +
                                "   ISNULL(MAX(EmploymentCheckId), 0) " +
                                "FROM [Cache].[EmploymentCheckMessageQueue] WITH (NOLOCK)",
                                commandType: CommandType.Text,
                                transaction: transaction)
                        ).FirstOrDefault();
                    }

                    _logger.LogInformation($"{thisMethodName}: Sql query returned EmploymentCheckLastHighestBatchId of {employmentCheckLastHighestBatchId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the employmentCheckLastHighestBatchId failed - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckLastHighestBatchId;
        }

        public async Task<IList<EmploymentCheckModel>> GetEmploymentCheckBatchById(
            long employmentCheckLastHighestBatchId,
            long batchSize)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckBatchById()";

            IEnumerable<EmploymentCheckModel> employmentChecksResult = null;
            IList<EmploymentCheckModel> employmentCheckModels = null;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@batchSize", batchSize);
                parameters.Add("@employmentCheckLastHighestBatchId", employmentCheckLastHighestBatchId);

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
                                _logger.LogInformation($"\n\n{DateTime.UtcNow} {thisMethodName}: Database query returned [{employmentChecksResult.Count()}] employment checks.");

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
                                _logger.LogError($"{thisMethodName}: Database query returned [0] employment checks.");
                                employmentCheckModels = new List<EmploymentCheckModel>(); // return an empty list rather than null
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the employmentCheckLastHighestBatchId failed - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckModels;
        }

        public async Task<int> StoreEmploymentCheckRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var thisMethodName = $"{ThisClassName}.StoreEmploymentCheckRequest()";

            int result = 0;
            try
            {
                if (employmentCheckCacheRequest != null)
                {
                    var dbRepository = new EmploymentCheckRepository(_logger, _connectionString, AzureResource);
                    if (dbRepository != null)
                    {
                        result = await dbRepository.Insert(employmentCheckCacheRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName} {ErrorMessagePrefix} The database call to get the employmentCheckLastHighestBatchId failed - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Store an individual employment check message in the database table queue
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="employmentCheckMessage"></param>
        /// <returns>Task</returns>
        public async Task<int> StoreEmploymentCheckMessage(
            EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"{ThisClassName}.StoreEmploymentCheckMessage()";

            int result = 0;
            try
            {
                if (employmentCheckMessage != null)
                {
                    var dbConnection = new DbConnection();
                    var sqlConnection = await dbConnection.CreateSqlConnection(
                        _logger,
                        _connectionString,
                        AzureResource,
                        _azureServiceTokenProvider);

                    if (sqlConnection != null)
                    {
                        _logger.LogInformation($"{thisMethodName}: Storing the Employment Check Message id: {employmentCheckMessage.Id} for employment check id {employmentCheckMessage.EmploymentCheckId}");

                        using (sqlConnection)
                        {
                            await sqlConnection.OpenAsync();

                            {
                                var transaction = sqlConnection.BeginTransaction();
                                {
                                    try
                                    {
                                        var parameter = new DynamicParameters();
                                        parameter.Add("@employmentCheckId", employmentCheckMessage.EmploymentCheckId, DbType.Int64);
                                        parameter.Add("@correlationId", employmentCheckMessage.CorrelationId, DbType.Guid);
                                        parameter.Add("@uln", employmentCheckMessage.Uln, DbType.Int64);
                                        parameter.Add("@nationalInsuranceNumber", employmentCheckMessage.NationalInsuranceNumber, DbType.String);
                                        parameter.Add("@payeScheme", employmentCheckMessage.PayeScheme, DbType.String);
                                        parameter.Add("@minDateTime", employmentCheckMessage.MinDateTime, DbType.DateTime);
                                        parameter.Add("@maxDateTime", employmentCheckMessage.MaxDateTime, DbType.DateTime);
                                        parameter.Add("@employed", employmentCheckMessage.Employed ?? false, DbType.Boolean);
                                        parameter.Add("@lastEmploymentCheck", employmentCheckMessage.LastEmploymentCheck, DbType.DateTime);
                                        parameter.Add("@responseHttpStatusCode", employmentCheckMessage.ResponseHttpStatusCode, DbType.Int16);
                                        parameter.Add("@responseMessage", employmentCheckMessage.ResponseMessage, DbType.String);
                                        parameter.Add("@createdOn", employmentCheckMessage.CreatedOn = DateTime.Now, DbType.DateTime);

                                        await sqlConnection.ExecuteAsync(
                                            "INSERT [SFA.DAS.EmploymentCheck.Database].[Cache].[EmploymentCheckMessageQueue] " +
                                            "       ( EmploymentCheckId,  CorrelationId, Uln,   NationalInsuranceNumber,  PayeScheme,  MinDateTime,  MaxDateTime,  Employed,  LastEmploymentCheck,  ResponseHttpStatusCode,  ResponseMessage,  CreatedOn) " +
                                            "VALUES (@employmentCheckId, @correlationId, @uln, @nationalInsuranceNumber, @payeScheme, @minDateTime, @maxDateTime, @employed, @lastEmploymentCheck, @ResponseHttpStatusCode, @ResponseMessage, @createdOn)",
                                            parameter,
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
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(result);
        }


        private SqlConnection GetSqlConnection(
            string connectionString,
            string azureResource)
        {
            var sqlConnection = new SqlConnection()
            {
                ConnectionString = connectionString,
                AccessToken = connectionString.Contains("Integrated Security") ? null : new AzureServiceTokenProvider().GetAccessTokenAsync(AzureResource).Result
            };

            return sqlConnection;
        }
    }
}