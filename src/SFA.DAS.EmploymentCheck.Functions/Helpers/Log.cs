using Microsoft.Extensions.Logging;
using System;

namespace SFA.DAS.EmploymentCheck.Functions.Helpers
{
    public static class Log
    {
        public static void WriteLog(
            ILogger logger,
            string methodName,
            string message)
        {
            try
            {
                var formattedMessage =
                    "\n-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n" +
                    $"CUSTOM LOGGING: { DateTime.Now } { methodName}: {message}" +
                    "\n-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n";

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
