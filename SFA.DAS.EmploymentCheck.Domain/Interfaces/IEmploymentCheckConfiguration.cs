using SFA.DAS.EmploymentCheck.Domain.Configuration;

namespace SFA.DAS.EmploymentCheck.Domain.Interfaces
{
    public interface IEmploymentCheckConfiguration
    {
        CompaniesHouseConfiguration CompaniesHouse { get; set; }
        string SubmissionEventApiAddress { get; set; }
    }
}
