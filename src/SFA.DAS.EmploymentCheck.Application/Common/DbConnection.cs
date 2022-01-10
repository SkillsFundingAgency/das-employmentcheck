using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Common
{
    public class DbConnection
    {
        #region Private members
        private const string ThisClassName = "\n\nSqlConnection";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";
        #endregion Private members

        #region CreateSqlConnection
        public async Task<SqlConnection> CreateSqlConnection(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var thisMethodName = $"{ThisClassName}.CreateSqlConnection()";

            SqlConnection sqlConnection = null;
            try
            {
                if (!String.IsNullOrEmpty(connectionString))
                {
                    if (!String.IsNullOrEmpty(azureResource))
                    {
                        sqlConnection = new SqlConnection(connectionString);
                        if (sqlConnection != null)
                        {
                            // no token for local db
                            if (azureServiceTokenProvider != null)
                            {
                                sqlConnection.AccessToken = await azureServiceTokenProvider.GetAccessTokenAsync(azureResource);
                            }
                            else
                            {
                                logger.LogInformation($"{thisMethodName}: azureServiceTokenProvider was not specified, call to azureServiceTokenProvider.GetAccessTokenAsync(azureResource) skipped.");
                            }
                        }
                        else
                        {
                            logger.LogInformation($"{thisMethodName}: Missing AzureResource string for the Employment Check Databasse.");
                        }
                    }
                    else
                    {
                        logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Missing SQL connecton string for the Employment Check Databasse.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return sqlConnection;
        }
        #endregion Constructors
    }
}