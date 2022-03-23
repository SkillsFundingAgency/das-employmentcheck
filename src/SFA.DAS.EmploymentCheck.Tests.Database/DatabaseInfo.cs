namespace SFA.DAS.EmploymentCheck.Tests.Database
{
    public class DatabaseInfo
    {
        public string ConnectionString { get; private set; }
        public string DatabaseName { get; private set; }

        public DatabaseInfo(string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString =
                    "Server=(localdb)\\MSSQLLocalDB;Database=SFA.DAS.EmploymentCheck.Database;Integrated Security=True;";
            }
            SetConnectionString(connectionString);
        }

        public void SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void SetDatabaseName(string databaseName)
        {
            DatabaseName = databaseName;
        }
    }
}
