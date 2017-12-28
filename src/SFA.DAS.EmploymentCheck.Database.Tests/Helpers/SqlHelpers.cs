using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace SFA.DAS.EmploymentCheck.Database.Tests.Helpers
{
    public static class SqlHelpers
    {
        public static async Task TruncateTables(IEnumerable<string> tables, string connectionString)
        {
            foreach (var table in tables)
            {
                await WithConnection(async c => await c.ExecuteAsync(sql: $"TRUNCATE TABLE {table}",
                    commandType: CommandType.Text), connectionString);
            }
        }

        private static async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                return await getData(connection);
            }
        }
    }
}
