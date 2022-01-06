using SFA.DAS.EmploymentCheck.Domain.Common;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    public class ApiResponseEntity
        : AuditableEntity
    {
        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }
    }
}
