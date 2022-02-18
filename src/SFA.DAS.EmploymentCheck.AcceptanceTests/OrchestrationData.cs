using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class OrchestrationData : IOrchestrationData
    {  
        public DurableOrchestrationStatus Status { get; set; }
    }
    public interface IOrchestrationData
    {
        DurableOrchestrationStatus Status { get; set; }
    }
}
