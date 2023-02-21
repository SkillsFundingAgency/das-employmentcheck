using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry
{
    public class TelemetryIntializer : ITelemetryInitializer
    {
        private readonly IHmrcApiTelemetrySanitizer _hmrcApiSanitizer;

        public TelemetryIntializer(IHmrcApiTelemetrySanitizer hmrcApiSanitizer)
        {
            _hmrcApiSanitizer = hmrcApiSanitizer;
        }

        public void Initialize(ITelemetry telemetry)
        {
            _hmrcApiSanitizer.ProcessHmrcApiTelemetry(telemetry);
        }        
    }
}