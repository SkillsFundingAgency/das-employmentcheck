using System;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.TestHelpers.AzureDurableFunctions
{
    public class OrchestrationStarterInfo
    {
        public string StarterName { get; }
        public Dictionary<string, object> StarterArgs { get; }
        public string OrchestrationName { get; }
        public TimeSpan? Timeout { get; }
        public string ExpectedCustomStatus { get; }

        public OrchestrationStarterInfo(
            string starterName,
            string orchestrationName = null,
            Dictionary<string, object> args = null,
            TimeSpan? timeout = null,
            string expectedCustomStatus = null)
        {
            if (string.IsNullOrEmpty(starterName)) throw new ArgumentException("Missing starter name");
            if (string.IsNullOrEmpty(orchestrationName)) throw new ArgumentException("Missing starter name");

            StarterName = starterName;
            OrchestrationName = orchestrationName;
            args ??= new Dictionary<string, object>();
            timeout ??= new TimeSpan(0, 60, 0);
            Timeout = timeout;
            StarterArgs = args;
            ExpectedCustomStatus = expectedCustomStatus;
        }
    }
}
