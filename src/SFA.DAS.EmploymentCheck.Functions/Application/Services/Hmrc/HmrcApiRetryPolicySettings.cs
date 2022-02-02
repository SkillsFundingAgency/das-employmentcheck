namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcApiRetryPolicySettings
    {
        public int TooManyRequestsRetryCount = 10;
        public int TransientErrorRetryCount = 2;
        public int TransientErrorDelayInMs = 1;
    }
}