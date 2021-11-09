using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;

namespace SFA.DAS.EmploymentCheck.Functions.Helpers
{
    public static class Log
    {
        public static void WriteLog(
            ILogger logger,
            string methodName,
            string message,
            IDurableOrchestrationContext context = null)
        {
            try
            {
                if (context != null && !context.IsReplaying)
                {
                    var formattedMessage =
                        "\n------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n" +
                        $"CUSTOM LOGGING: {context.CurrentUtcDateTime} { methodName}: {message}" +
                        "\n------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n";

                    logger.LogInformation(formattedMessage);
                }
            }
            catch
            {
                // TODO: How to display exception if logger argument is null or context is null?
                throw;
            }
        }

        public static void WriteLog<T>(
            ILogger<T> logger,
            string methodName,
            string message,
            IDurableOrchestrationContext context = null)
        {
            try
            {
                if (context != null && !context.IsReplaying)
                {
                    var formattedMessage =
                        "\n------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n" +
                        $"CUSTOM LOGGING: {DateTime.UtcNow} {methodName}: {message}" +
                        "\n------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n";

                    logger.LogInformation(formattedMessage);
                }

            }
            catch
            {
                // TODO: How to display exception if logger argument is null?
                throw;
            }
        }

    }
}
