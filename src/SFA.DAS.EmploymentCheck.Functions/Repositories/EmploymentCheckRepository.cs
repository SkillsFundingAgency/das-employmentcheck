using Ardalis.GuardClauses;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class EmploymentCheckRepository
        : IEmploymentCheckRepository
    {
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        public EmploymentCheckRepository(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null
        )
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task InsertOrUpdate(Models.EmploymentCheck check)
        {
            Guard.Against.Null(check, nameof(check));
            var checkType = check.GetType();

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
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
                            // Check there's a LastUpdatedOn property on the object before setting the timestamp
                            if (checkType.GetProperty("LastUpdatedOn") != null) { check.LastUpdatedOn = DateTime.Now; }
                            await sqlConnection.UpdateAsync(check, tran);
                        }
                        else
                        {
                            // Check there's a CreatedOn property on the object before setting the timestamp
                            if (checkType.GetProperty("CreatedOn") != null) { check.CreatedOn = DateTime.Now; }
                            await sqlConnection.InsertAsync(check, tran);
                        }

                        await tran.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tran.RollbackAsync();
                        throw;
                    }
                }

            }
        }

        public async Task Save(Models.EmploymentCheck check)
        {
            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.InsertAsync(check);
        }
    }
}