using Dapper;
using Dapper.Contrib.Extensions;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace app_levy_data_seeder
{
    public class DataAccess
    {
        private readonly string _connectionString;

        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> Insert<T>(T entity) where T : class
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            return await dbConnection.InsertAsync(entity);
        }

        public async Task Update<T>(T entity) where T : class
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.UpdateAsync(entity);
        }

        public async Task DeleteAll(string table)
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.ExecuteAsync($"DELETE {table}");
        }
    }
}
