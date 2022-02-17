using System;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public interface IHmrcApiRetryPolicies
    {
        Task<AsyncPolicyWrap> GetAll(Func<Task> onRetry);
        Task ReduceRetryDelay();
        Task<AsyncPolicy> GetTokenRetrievalRetryPolicy();
    }
}