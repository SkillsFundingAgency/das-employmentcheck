using SFA.DAS.EmploymentCheck.Application.Common.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Common.Interfaces
{
    public interface IHmrcApiOptionsRepository
    {
        Task<HmrcApiRateLimiterOptions> GetHmrcRateLimiterOptions();

        Task ReduceDelaySetting(HmrcApiRateLimiterOptions options);

        Task IncreaseDelaySetting(HmrcApiRateLimiterOptions options);

        Task CreateDefaultOptions();
    }
}