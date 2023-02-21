using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Application.Telemetry
{
    public class TelemetryWrapper : ITelemetryClient
    {
        private readonly TelemetryClient _telemetryClient;

        public TelemetryWrapper(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            _telemetryClient.TrackDependency(dependencyTypeName, dependencyName, data, startTime, duration, success);
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            _telemetryClient.TrackEvent(eventName, properties, metrics);
        }
    }
}
