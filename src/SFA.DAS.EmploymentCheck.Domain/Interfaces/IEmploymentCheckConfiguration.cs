using SFA.DAS.EmploymentCheck.Domain.Configuration;

namespace SFA.DAS.EmploymentCheck.Domain.Interfaces
{
    public interface IEmploymentCheckConfiguration
    {
        string SubmissionEventApiAddress { get; set; }
    }
}
