using System;
using AutoFixture;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class RepositoryTestBase
    {
        protected Fixture Fixture;
        protected ApplicationSettings Settings;
        protected IList<object> ToBeDeleted = new List<object>();
        private const string AzureResource = "https://database.windows.net/";

        [SetUp]
        public void SetUp()
        {
            Fixture = new Fixture();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false).Build();

            Settings = new ApplicationSettings();
            config.Bind(Settings);
        }

        [TearDown]
        public async Task CleanUp()
        {
            await DeleteAll();
        }

        private async Task DeleteAll()
        {
            foreach (var entity in ToBeDeleted)
            {
               // await Delete(entity);
            }
        }

        public async Task<T> Get<T>(object id) where T : class
        {
            await using var dbConnection = GetSqlConnection();
            return await dbConnection.GetAsync<T>(id);
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : class
        {
            await using var dbConnection = GetSqlConnection();
            return await dbConnection.GetAllAsync<T>();
        }

        public async Task<int> Insert<T>(T entity) where T : class
        {
            ToBeDeleted.Add(entity);
            await using var dbConnection = GetSqlConnection();
            return await dbConnection.InsertAsync(entity);
        }

        public async Task Update<T>(T entity) where T : class
        {
            await using var dbConnection = GetSqlConnection();
            await dbConnection.UpdateAsync(entity);
        }

        public async Task Delete<T>(T entity) where T : class
        {
            await using var dbConnection = GetSqlConnection();
            await dbConnection.DeleteAsync(entity);
        }

        private SqlConnection GetSqlConnection()
        {
            return new SqlConnection { ConnectionString = Settings.DbConnectionString, AccessToken = Settings.DbConnectionString.Contains("Integrated Security") ? null : new AzureServiceTokenProvider().GetAccessTokenAsync(AzureResource).Result };
        }

        // How do we test the additional methods we use in our repository that are not
        // the standard Dapper one's?

        // e.g. the InsertOrUpdate (InsertOrUpdate) as shown below:
        //public async Task InsertOrUpdate(Models.EmploymentCheck check)
        //{
        //    var dbConnection = new DbConnection();
        //    await using var sqlConnection = await dbConnection.CreateSqlConnection(
        //        _connectionString,
        //        _azureServiceTokenProvider);

        //    await using var tran = await sqlConnection.BeginTransactionAsync();
        //    var existingItem = await sqlConnection.GetAsync<Models.EmploymentCheck>(check.Id);
        //    if (existingItem != null) await sqlConnection.UpdateAsync(check);
        //    else await sqlConnection.InsertAsync(check);

        //    await tran.CommitAsync();
        //}

        // Is it ok to just reference the repository from here?



    }
}