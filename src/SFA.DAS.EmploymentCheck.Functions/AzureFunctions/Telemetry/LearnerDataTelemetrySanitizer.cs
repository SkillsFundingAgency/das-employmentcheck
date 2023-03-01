using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry
{
    public class LearnerDataTelemetrySanitizer : ILearnerDataTelemetrySanitizer
    {
        public void ProcessLearnerDataTelemetry(ITelemetry telemetry)
        {
            if (telemetry is DependencyTelemetry dependency)
            {
                string marker = "?ulns=";

                var indexOfMarker = dependency.Name.LastIndexOf(marker);
                if (indexOfMarker >= 0)
                {
                    dependency.Name = dependency.Name[..(indexOfMarker)];
                }

                indexOfMarker = dependency.Data.LastIndexOf(marker);
                if (indexOfMarker >= 0)
                {
                    dependency.Data = dependency.Data[..(indexOfMarker)];
                }
            };
        }
    }
}