using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry
{
    public class TelemetryIntializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            //ProcessHmrcApiTelemetry(telemetry);
        }

        private static void ProcessHmrcApiTelemetry(ITelemetry telemetry)
        {
            if (telemetry is DependencyTelemetry dependency && dependency.Target.Contains("hmrc.gov.uk"))
            {
                var indexOfMarker = dependency.Name.LastIndexOf("/paye/");
                if (indexOfMarker >= 0)
                {
                    dependency.Name = dependency.Name[..(indexOfMarker + "/paye/".Length)];
                }

                indexOfMarker = dependency.Data.LastIndexOf("/paye/");
                if (indexOfMarker >= 0)
                {
                    dependency.Data = dependency.Data[..(indexOfMarker + "/paye/".Length)];
                }
            };
        }
    }
}