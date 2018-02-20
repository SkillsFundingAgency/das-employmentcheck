﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Infrastructure
{
    public abstract class DbConnection
    {
        private readonly string _connectionString;

        public DbConnection(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        protected async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await getData(connection);
            }
        }
    }
}
