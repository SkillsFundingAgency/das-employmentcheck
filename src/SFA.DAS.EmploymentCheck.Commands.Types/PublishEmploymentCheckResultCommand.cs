using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Infrastructure.DistributedLock;
using SFA.DAS.EmploymentCheck.Infrastructure.Logging;
using System;

namespace SFA.DAS.EmploymentCheck.Commands.Types
{
    public class PublishEmploymentCheckResultCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public Guid CorrelationId { get; }
        public bool? EmploymentResult { get; }
        public DateTime CheckDate { get; }
        public string ErrorType { get; }

        public PublishEmploymentCheckResultCommand(Guid correlationId, bool? employmentResult, DateTime checkDate, string errorType)
        {
            CorrelationId = correlationId;
            EmploymentResult = employmentResult;
            CheckDate = checkDate;
            ErrorType = errorType;
        }

        public string LockId => $"{nameof(CorrelationId)}_{CorrelationId}";

        [JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"EmploymentCheck {nameof(PublishEmploymentCheckResultCommand)} event with CorrelationId {CorrelationId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
