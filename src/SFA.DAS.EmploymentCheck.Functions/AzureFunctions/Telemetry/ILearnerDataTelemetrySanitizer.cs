using Microsoft.ApplicationInsights.Channel;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry
{
    public interface ILearnerDataTelemetrySanitizer
    {
        public void ProcessLearnerDataTelemetry(ITelemetry telemetry);
    }
}
