using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IHmrcApiOptionsRepository
    {
        Task ReduceDelaySetting(int value);
        Task IncreaseDelaySetting(int value);
        Task<HmrcApiRateLimiterOptions> GetHmrcRateLimiterOptions();
    }
}