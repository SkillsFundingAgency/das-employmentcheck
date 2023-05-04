using Polly.Wrap;

namespace SFA.DAS.EmploymentCheck.Application.Services
{
    public interface IApiRetryPolicies
    {
        AsyncPolicyWrap GetAll();
    }
}
