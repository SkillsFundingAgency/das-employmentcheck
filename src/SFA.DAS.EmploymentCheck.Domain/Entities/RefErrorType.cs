using Dapper.Contrib.Extensions;
using SFA.DAS.EmploymentCheck.Domain.Common;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    [Table("Cache.RefErrorType")]
    public class RefErrorType
        : Entity
    {
        public RefErrorType() { }

        public RefErrorType(
            long id,
            long employmentCheckId,
            string source,
            string message,
            int maxRetries)
        {
            Id = id;
            EmploymentCheckId = employmentCheckId;
            Source = source;
            Message = message;
            MaxRetries = maxRetries;
        }

        public long EmploymentCheckId { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public int MaxRetries { get; set; }
    }
}

