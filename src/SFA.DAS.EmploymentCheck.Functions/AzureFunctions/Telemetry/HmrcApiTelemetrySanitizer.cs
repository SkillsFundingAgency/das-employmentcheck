using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry
{
    public class HmrcApiTelemetrySanitizer : IHmrcApiTelemetrySanitizer
    {
        public void ProcessHmrcApiTelemetry(ITelemetry telemetry)
        {
            if (telemetry is DependencyTelemetry dependency)
            {
                var indexOfMarker = dependency.Name.LastIndexOf("/epaye/");
                if (indexOfMarker >= 0)
                {
                    dependency.Name = dependency.Name[..(indexOfMarker + "/epaye/".Length)];
                }

                indexOfMarker = dependency.Data.LastIndexOf("/epaye/");
                if (indexOfMarker >= 0)
                {
                    dependency.Data = dependency.Data[..(indexOfMarker + "/epaye/".Length)];
                }
            };
        }
    }
}
