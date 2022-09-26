using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace SFA.DAS.EmploymentCheck.Functions
{
    /*
    * Custom TelemetryInitializer that removes the sensitive info from hmrc api call logs
    */
    public class HmrcTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            switch (telemetry)
            {
                case RequestTelemetry requestTelemetry:
                    {
                        Regex.Replace(requestTelemetry.Name, "epaye\\/.*", "epaye/");
                    }
                    break;
                case TraceTelemetry traceTelemetry:
                    {
                        Regex.Replace(traceTelemetry.Message, "epaye\\/.*", "epaye/");
                    }
                    break;
            }
        }
    }
}