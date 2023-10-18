using System;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;

namespace SFA.DAS.EmploymentCheck.Application.Services.Hmrc
{
    public interface IHmrcApiRetryPolicies
    {
        AsyncPolicyWrap GetAll(Func<Task> onRetry);
        void ReduceRetryDelay();
        Task<AsyncPolicy> GetTokenRetrievalRetryPolicy();
        Task DelayApiExecutionByRetryPolicy();
    }
}