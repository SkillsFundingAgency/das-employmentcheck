using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IHmrcApiOptionsRepository
    {
        int GetRequestDelayInMsSetting();
        void UpdateRequestDelaySetting(int value);
        HmrcApiRateLimiterOptions GetHmrcRateLimiterOptions();
    }
}