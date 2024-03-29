﻿using Ardalis.GuardClauses;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public class EmploymentCheckRepository : IEmploymentCheckRepository
    {
        private readonly ILogger<EmploymentCheckRepository> _logger;
        private readonly ApplicationSettings _settings;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckRepository(
            ApplicationSettings applicationSettings,
            ILogger<EmploymentCheckRepository> logger,
            AzureServiceTokenProvider azureServiceTokenProvider = null
        )
        {
            _logger = logger;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _settings = applicationSettings;
        }

        public async Task<Models.EmploymentCheck> GetEmploymentCheck()
        {
            Models.EmploymentCheck employmentCheck;

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _settings.DbConnectionString,
                _azureServiceTokenProvider))
            {
                await sqlConnection.OpenAsync();
                var transaction = sqlConnection.BeginTransaction();

                try
                {
                    employmentCheck = (await sqlConnection.QueryAsync<Models.EmploymentCheck>(
                            sql: "SELECT TOP (1) " +
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
                            commandType: CommandType.Text,
                            transaction: transaction)).FirstOrDefault();

                    if (employmentCheck != null)
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

                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    _logger.LogError($"EmploymentCheckService.GetEmploymentCheck(): ERROR: An error occurred reading the employment check. Exception [{e}]");
                    throw;
                }
            }

            return employmentCheck;
        }

        public async Task<Models.EmploymentCheck> GetResponseEmploymentCheck()
        {
            Models.EmploymentCheck employmentCheck;

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _settings.DbConnectionString,
                _azureServiceTokenProvider))
            {
                await sqlConnection.OpenAsync();
                var transaction = sqlConnection.BeginTransaction();

                try
                {
                    employmentCheck = (await sqlConnection.QueryAsync<Models.EmploymentCheck>(
                            sql: "SELECT TOP (1) " +
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
                                "[MessageSentDate], " +
                                "[ErrorType], " +
                                "[CreatedOn], " +
                                "[LastUpdatedOn] " +
                                "FROM [Business].[EmploymentCheck] AEC " +
                                "WHERE AEC.RequestCompletionStatus = 2 " +
                                "AND MessageSentDate IS NULL " +
                                "ORDER BY CreatedOn ",
                            commandType: CommandType.Text,
                            transaction: transaction)).FirstOrDefault();

                    if (employmentCheck != null)
                    {
                        var dateTimeNow = DateTime.Now;
                        var parameter = new DynamicParameters();
                        parameter.Add("@Id", employmentCheck.Id, DbType.Int64);
                        parameter.Add("@messageSentDate", dateTimeNow, DbType.DateTime);
                        parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

                        await sqlConnection.ExecuteAsync(
                            "UPDATE [Business].[EmploymentCheck] " +
                            "SET MessageSentDate = @messageSentDate, LastUpdatedOn = @lastUpdatedOn " +
                            "WHERE Id = @Id ",
                            parameter,
                            commandType: CommandType.Text,
                            transaction: transaction);

                        employmentCheck.MessageSentDate = dateTimeNow;
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    _logger.LogError($"EmploymentCheckService.GetResponseEmploymentCheck(): ERROR: An error occurred reading the employment check. Exception [{e}]");
                    throw;
                }
            }

            return employmentCheck;
        }

        public async Task InsertOrUpdate(Models.EmploymentCheck check)
        {
            Guard.Against.Null(check, nameof(check));

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _settings.DbConnectionString,
                _azureServiceTokenProvider)
            )
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));
                await sqlConnection.OpenAsync();
                using (var tran = await sqlConnection.BeginTransactionAsync())
                {
                    try
                    {
                        var existingItem = await sqlConnection.GetAsync<Models.EmploymentCheck>(check.Id, tran);
                        if (existingItem != null)
                        {
                            check.LastUpdatedOn = DateTime.Now;
                            await sqlConnection.UpdateAsync(check, tran);
                        }
                        else
                        {
                            try
                            {
                                check.CreatedOn = DateTime.Now;
                                await sqlConnection.InsertAsync(check, tran);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"{nameof(AccountsResponseRepository)} Exception caught: {ex.Message}. {ex.StackTrace}");
                                throw;
                            }
                        }

                        await tran.CommitAsync();
                    }
                    catch
                    {
                        await tran.RollbackAsync();
                    }
                }
            }
        }

        public async Task UpdateEmploymentCheckAsComplete(Models.EmploymentCheck check, IUnitOfWork unitOfWork)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Id", check.Id, DbType.Int64);
            parameter.Add("@employed", check.Employed, DbType.Boolean);
            parameter.Add("@requestCompletionStatus", (short)ProcessingCompletionStatus.Completed, DbType.Int16);
            parameter.Add("@errorType", check.ErrorType, DbType.String);
            parameter.Add("@lastUpdatedOn", DateTime.Now, DbType.DateTime);

            const string sql = "UPDATE [Business].[EmploymentCheck] " +
                               "SET Employed = @employed, RequestCompletionStatus = @requestCompletionStatus, ErrorType = @errorType, LastUpdatedOn = @lastUpdatedOn, MessageSentDate = null " +
                               "WHERE Id = @Id AND (Employed IS NULL OR Employed = 0) ";

            await unitOfWork.ExecuteSqlAsync(sql, parameter);
        }
    }
}