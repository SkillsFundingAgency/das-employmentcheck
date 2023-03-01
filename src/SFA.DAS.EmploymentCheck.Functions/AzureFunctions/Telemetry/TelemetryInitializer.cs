using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry
{
    public class TelemetryIntializer : ITelemetryInitializer
    {
        private readonly IHmrcApiTelemetrySanitizer _hmrcApiSanitizer;
        private readonly ILearnerDataTelemetrySanitizer _learnerDataSanitizer;

        public TelemetryIntializer(IHmrcApiTelemetrySanitizer hmrcApiSanitizer, ILearnerDataTelemetrySanitizer learnerDataSanitizer)
        {
            _hmrcApiSanitizer = hmrcApiSanitizer;
            _learnerDataSanitizer = learnerDataSanitizer;
        }

        public void Initialize(ITelemetry telemetry)
        {
            _hmrcApiSanitizer.ProcessHmrcApiTelemetry(telemetry);
            _learnerDataSanitizer.ProcessLearnerDataTelemetry(telemetry);
        }        
    }
}