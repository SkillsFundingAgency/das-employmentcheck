using Polly.Wrap;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services
{
    public interface IApiRetryPolicies
    {
        Task<AsyncPolicyWrap> GetAll(string rowKey);
    }
}
