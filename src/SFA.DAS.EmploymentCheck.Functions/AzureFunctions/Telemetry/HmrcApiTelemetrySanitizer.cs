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
