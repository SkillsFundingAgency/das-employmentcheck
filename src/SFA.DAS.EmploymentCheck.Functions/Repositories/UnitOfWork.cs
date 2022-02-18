using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using DbConnection = SFA.DAS.EmploymentCheck.Functions.Application.Helpers.DbConnection;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private SqlConnection _sqlConnection;
        private DbTransaction _transaction;

        public UnitOfWork(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null
        )
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task ExecuteSqlAsync(string sql, DynamicParameters parameter = null)
        {
            await _sqlConnection.ExecuteAsync(sql, parameter,  _transaction, commandType: CommandType.Text);
        }

        public async Task BeginAsync()
        {
            var dbConnection = new DbConnection();
            _sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);

            await _sqlConnection.OpenAsync();
            _transaction = await _sqlConnection.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
            await DisposeAsync();
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
            await DisposeAsync();
        }

        private async Task DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();
            _transaction = null;
        }
        public void Dispose()
        {
            _transaction?.DisposeAsync();
            _transaction = null;
        }

        public async Task UpdateAsync<T>(T entity) where T : class
        {
            await _sqlConnection.UpdateAsync(entity, _transaction);
        }

        public async Task InsertAsync<T>(T entity) where T : class
        {
            await _sqlConnection.InsertAsync(entity, _transaction);
        }

    }
}
