using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IHmrcApiOptionsRepository
    {
        HmrcApiRateLimiterOptions GetHmrcRateLimiterOptions();
        ApiRetryDelaySettings ReduceDelaySetting(HmrcApiRateLimiterOptions options);
        ApiRetryDelaySettings IncreaseDelaySetting(HmrcApiRateLimiterOptions options);
    }
}