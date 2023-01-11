using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly ILogger<UnitOfWork> _logger;
        private SqlConnection _sqlConnection;
        private DbTransaction _transaction;
        private bool _isTransactionActive;

        public UnitOfWork(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider = null,
            ILogger<UnitOfWork> logger = null
        )
        {
            _logger = logger;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task ExecuteSqlAsync(string sql, DynamicParameters parameter = null)
        {
            await _sqlConnection.ExecuteAsync(sql, parameter, _transaction, commandType: CommandType.Text);
        }

        public async Task UpdateAsync<T>(T entity) where T : class
        {
            await _sqlConnection.UpdateAsync(entity, _transaction);
        }

        public async Task InsertAsync<T>(T entity) where T : class
        {
            await _sqlConnection.InsertAsync(entity, _transaction);
        }

        public async Task BeginAsync()
        {
            var dbConnection = new DbConnection();
            _sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);

            await _sqlConnection.OpenAsync();
            _transaction = await _sqlConnection.BeginTransactionAsync();
            _isTransactionActive = _transaction.Connection.State == ConnectionState.Open;
        }

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
            await DisposeAsync();
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null && _isTransactionActive) 
                    await _transaction.RollbackAsync();

                await DisposeAsync();
            }
            catch (Exception e)
            {
                _logger?.LogError($"{nameof(UnitOfWork)}: failed during transaction rollback [{e}]");
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();

            if (_sqlConnection != null)
            {
                if (_sqlConnection.State != ConnectionState.Closed) 
                    await _sqlConnection.CloseAsync();

                await _sqlConnection.DisposeAsync();
            }

            _transaction = null;
            _sqlConnection = null;
        }
    }
}
