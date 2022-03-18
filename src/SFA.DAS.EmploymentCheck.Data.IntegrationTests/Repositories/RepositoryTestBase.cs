using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Tests.Database;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class RepositoryTestBase
    {
        protected Fixture Fixture;
        protected ApplicationSettings Settings;
        protected IUnitOfWork UnitOfWorkInstance;
        private string _databaseName; 
        private SqlDatabase _database;

        [SetUp]
        public void SetUp()
        {
            Fixture = new Fixture();

            SqlDatabaseModel.Update();

            _databaseName = Fixture.Create<string>();

            _database = new SqlDatabase(_databaseName);

            Settings = new ApplicationSettings
            {
                DbConnectionString = _database.DatabaseInfo.ConnectionString
            };

            UnitOfWorkInstance = new Data.Repositories.UnitOfWork(Settings);

            AssertionOptions.AssertEquivalencyUsing(options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>();
                options.Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTimeOffset>();
                return options;
            });
        }


        [TearDown]
        public void CleanUp()
        {
            _database.Dispose();
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

        public async Task Delete<T>(T entity) where T : class
        {
            await using var dbConnection = GetSqlConnection();
            await dbConnection.DeleteAsync(entity);
        }

        private SqlConnection GetSqlConnection()
        {
            return new SqlConnection { ConnectionString = Settings.DbConnectionString };
        }
    }
}