
namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class ApiRetryOptions 
    {
        public int TooManyRequestsRetryCount { get; set; } 
        public int TransientErrorRetryCount { get; set; } 
        public int TransientErrorDelayInMs { get; set; } 
        public int TokenRetrievalRetryCount { get; set; } 
    }
}
