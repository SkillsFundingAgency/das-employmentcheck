using Ardalis.GuardClauses;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class Repository<T>
        : IRepository<T> where T : class
    {
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly DbConnection _dbConnection;

        public Repository(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null
        )
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
            _dbConnection = new DbConnection();
        }


        //public async Task<IEnumerable<T>> GetAll()
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<IList<T>> GetAll(T entity)
        public async Task<IEnumerable<T>> GetAll<T>() where T : class
        {
            await using (var sqlConnection = await _dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();
                return await sqlConnection.GetAllAsync<T>();
            }
        }

        public async Task<T> GetById<T>(long id) where T : class
        {
            await using (var sqlConnection = await _dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();
                return await sqlConnection.GetAsync<T>(id, null);
            }
        }

        public async Task InsertAsync(T entity)
        {
            await using (var sqlConnection = await _dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();
                await sqlConnection.InsertAsync<T>(entity, null);
            }
        }

        public async Task UpdateAsync(T entity)
        {
            await using (var sqlConnection = await _dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();
                await sqlConnection.UpdateAsync<T>(entity, null);
            }
        }

        public async Task DeleteAsync(T entity)
        {
            await using (var sqlConnection = await _dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                await sqlConnection.OpenAsync();
                await sqlConnection.DeleteAsync<T>(entity, null);
            }
        }

        public async Task Save(T entity)
        {
            await using (var sqlConnection = await _dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider)
            )
            {
                var responseType = entity.GetType();

                await sqlConnection.OpenAsync();
                using var tran = await sqlConnection.BeginTransactionAsync();
                try
                {
                    //var existingItem = await sqlConnection.GetAsync<T>(responseType, tran);
                    var existingItem = await GetById(((Entity)entity).Id);
                    if (existingItem != null)
                    {
                        // If there's a LastUpdatedOn property on the object set it's value to now
                        var lastUpdateProperty = responseType.GetProperty("LastUpdatedOn");
                        if(lastUpdateProperty != null)
                        {
                            lastUpdateProperty.SetValue(entity, DateTime.Now);
                        }
                        await sqlConnection.UpdateAsync(entity, tran);
                    }
                    else
                    {
                        // If there's a CreatedOn property on the object set it's value to now
                        var lastUpdateProperty = responseType.GetProperty("CreatedOn");
                        if (lastUpdateProperty != null)
                        {
                            lastUpdateProperty.SetValue(entity, DateTime.Now);
                        }
                        await sqlConnection.InsertAsync(entity, tran);
                    }

                    await tran.CommitAsync();
                }
                catch(Exception ex)
                {
                    await tran.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
