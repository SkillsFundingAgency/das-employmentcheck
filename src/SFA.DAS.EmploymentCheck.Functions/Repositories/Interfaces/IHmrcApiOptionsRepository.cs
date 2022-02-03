using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IHmrcApiOptionsRepository
    {
        Task<HmrcApiRateLimiterOptions> GetHmrcRateLimiterOptions();
        Task ReduceDelaySetting(HmrcApiRateLimiterOptions options);
        Task IncreaseDelaySetting(HmrcApiRateLimiterOptions options);
    }
}