using Microsoft.ApplicationInsights.Channel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry
{
    public interface IHmrcApiTelemetrySanitizer
    {
        public void ProcessHmrcApiTelemetry(ITelemetry telemetry);
    }
}
