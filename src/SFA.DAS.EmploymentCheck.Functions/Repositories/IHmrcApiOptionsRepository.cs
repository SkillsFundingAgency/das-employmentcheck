using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IHmrcApiOptionsRepository
    {
        void ReduceDelaySetting(int value);
        void IncreaseDelaySetting(int value);
        HmrcApiRateLimiterOptions GetHmrcRateLimiterOptions();
    }
}