using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Configuration;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Repositories
{
    public class RepositoryTestBase
    {
        protected Fixture Fixture;
        protected EmploymentCheckSettings Settings;
        private const string AzureResource = "https://database.windows.net/";

        [SetUp]
        public void SetUp()
        {
            Fixture = new Fixture();
            
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false).Build();
            
            Settings = new EmploymentCheckSettings();
            config.Bind(Settings);
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
            await using var dbConnection = GetSqlConnection();
            return await dbConnection.InsertAsync(entity);
        }
        
        public async Task Update<T>(T entity) where T : class
        {
            await using var dbConnection = GetSqlConnection();
            await dbConnection.UpdateAsync(entity);
        }
        
        private SqlConnection GetSqlConnection()
        {
            return new SqlConnection { ConnectionString = Settings.DbConnectionString, AccessToken = Settings.DbConnectionString.Contains("Integrated Security") ? null : new AzureServiceTokenProvider().GetAccessTokenAsync(AzureResource).Result };
        }
    }
}