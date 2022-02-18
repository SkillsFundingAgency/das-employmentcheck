using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Data.SqlClient;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Database
{
    public class SqlDatabase : IDisposable
    {
        private bool _isDisposed;
        public DatabaseInfo DatabaseInfo { get; } = new DatabaseInfo();

        public SqlDatabase(string dbName)
        {
            DatabaseInfo.SetDatabaseName(dbName);
            //CreateTestDatabase();
        }

        private void CreateTestDatabase()
        {
            Directory.CreateDirectory("C:\\temp");
            DatabaseInfo.SetConnectionString(
                @$"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog={DatabaseInfo.DatabaseName};Integrated Security=True;MultipleActiveResultSets=True;Pooling=False;Connect Timeout=30;");

            using var dbConn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;MultipleActiveResultSets=true");
            try
            {
                var sql = @$"CREATE DATABASE [{DatabaseInfo.DatabaseName}] ON PRIMARY
                     (NAME = [{DatabaseInfo.DatabaseName}_Data],
                      FILENAME = 'C:\\temp\\{DatabaseInfo.DatabaseName}.mdf')
                      LOG ON (NAME = [{DatabaseInfo.DatabaseName}_Log],
                      FILENAME = 'C:\\temp\\{DatabaseInfo.DatabaseName}.ldf')";
                using var cmd = new SqlCommand(sql, dbConn);
                dbConn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(CreateTestDatabase)} failed. Exception={ex}");
            }
            finally
            {
                if (dbConn.State == ConnectionState.Open)
                {
                    dbConn.Close();
                }
            }

            using var dbConnection = new SqlConnection(DatabaseInfo.ConnectionString);
            dbConnection.Open();
        }

        private void DeleteTestDatabase()
        {
            try
            {
                var files = new List<string>();
                using var dbConn = new SqlConnection(DatabaseInfo.ConnectionString);
                using var cmd = new SqlCommand("SELECT DB_NAME()", dbConn);
                dbConn.Open();
                var dbName = cmd.ExecuteScalar();
                cmd.CommandText = "SELECT filename FROM sysfiles";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        files.Add((string)reader["filename"]);
                    }
                }
                cmd.CommandText = $"ALTER DATABASE [{dbName}] SET OFFLINE WITH ROLLBACK IMMEDIATE";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"EXEC sp_detach_db '{dbName}', 'true';";
                cmd.ExecuteNonQuery();
                dbConn.Close();

                files.ForEach(DeleteFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(SqlDatabase)}] {nameof(DeleteTestDatabase)} exception thrown: {ex.Message}");
                throw ex;
            }
        }

        private static void DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(SqlDatabase)}] {nameof(DeleteFile)} exception thrown: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                DeleteTestDatabase();
            }

            _isDisposed = true;
        }
    }
}
