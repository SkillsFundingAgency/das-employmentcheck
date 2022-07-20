using Microsoft.WindowsAzure.Storage.Table;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class ApiRetryOptions : TableEntity
    {
        public int TooManyRequestsRetryCount { get; set; } = 10;
        public int TransientErrorRetryCount { get; set; } = 3;
        public int TransientErrorDelayInMs { get; set; } = 10000;
        public int TokenRetrievalRetryCount { get; set; } = 2;
    }
}
