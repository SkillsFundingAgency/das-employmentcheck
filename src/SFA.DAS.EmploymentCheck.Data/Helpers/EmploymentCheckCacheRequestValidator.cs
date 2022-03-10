using FluentValidation;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.Helpers
{
    public class EmploymentCheckCacheRequestValidator
        : AbstractValidator<EmploymentCheckCacheRequest>
    {
        public EmploymentCheckCacheRequestValidator()
        {
            RuleFor(employmentCheckCacheRequest => employmentCheckCacheRequest.ApprenticeEmploymentCheckId).NotEmpty();
            RuleFor(employmentCheckCacheRequest => employmentCheckCacheRequest.Nino).NotEmpty();
            RuleFor(employmentCheckCacheRequest => employmentCheckCacheRequest.PayeScheme).NotEmpty();
            RuleFor(employmentCheckCacheRequest => employmentCheckCacheRequest.MinDate).NotEqual(System.DateTime.MinValue);
            RuleFor(employmentCheckCacheRequest => employmentCheckCacheRequest.MaxDate).NotEqual(System.DateTime.MinValue);
        }
    }
}
