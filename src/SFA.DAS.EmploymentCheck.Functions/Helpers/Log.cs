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
                // Context is only applicable if this is called from an Orchestration function
                DateTime? currentUtcDateTime = null;
                if (context != null && !context.IsReplaying)
                {
                    currentUtcDateTime = context.CurrentUtcDateTime;
                }
                else
                {
                    currentUtcDateTime = DateTime.UtcNow;
                }

                var formattedMessage =
                    "\n------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n" +
                    $"CUSTOM LOGGING: {currentUtcDateTime} { methodName}: {message}" +
                    "\n------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n";

                logger.LogInformation(formattedMessage);

            }
            catch
            {
                // TODO: How to display exception if logger argument is null?
                throw;
            }
        }
    }
}
