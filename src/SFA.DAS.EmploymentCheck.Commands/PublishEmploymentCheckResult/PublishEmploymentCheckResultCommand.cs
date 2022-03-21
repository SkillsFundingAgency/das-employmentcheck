using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Infrastructure.DistributedLock;
using SFA.DAS.EmploymentCheck.Infrastructure.Logging;

namespace SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult
{
    public class PublishEmploymentCheckResultCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public PublishEmploymentCheckResultCommand(Data.Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Data.Models.EmploymentCheck EmploymentCheck { get; }
        public string LockId => $"{nameof(Data.Models.EmploymentCheck)}_{EmploymentCheck.Id}";

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"EmploymentCheck {nameof(PublishEmploymentCheckResultCommand)} event with CheckId {EmploymentCheck.Id}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
