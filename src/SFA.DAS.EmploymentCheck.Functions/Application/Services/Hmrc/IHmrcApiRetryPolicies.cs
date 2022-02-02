using System;
using System.Threading.Tasks;
using Polly.Wrap;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public interface IHmrcApiRetryPolicies
    {
        AsyncPolicyWrap GetAll(Func<Task> onRetry);
    }
}