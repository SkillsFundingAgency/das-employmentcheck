using System;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Logging
{
    public class Log
    {
        public Func<string> OnProcessing { get; set; }
        public Func<string> OnProcessed { get; set; }
        public Func<string> OnError { get; set; }

        public Log()
        {
            OnProcessing = () => "";
            OnProcessed = () => "";
            OnError = () => "";
        }
    }
}
