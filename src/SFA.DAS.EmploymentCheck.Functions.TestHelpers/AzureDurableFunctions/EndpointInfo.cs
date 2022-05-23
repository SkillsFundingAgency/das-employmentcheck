using System;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.TestHelpers.AzureDurableFunctions
{
    public class EndpointInfo
    {
        public string StarterName { get; }
        public Dictionary<string, object> StarterArgs { get; }

        public EndpointInfo(
            string starterName,
            Dictionary<string, object> args = null)
        {
            if (string.IsNullOrEmpty(starterName)) throw new ArgumentException("Missing starter name");

            StarterName = starterName;
            args ??= new Dictionary<string, object>();
            StarterArgs = args;
        }
    }
}