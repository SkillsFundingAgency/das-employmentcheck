using SFA.DAS.EmploymentCheck.Domain.Interfaces;

namespace SFA.DAS.EmploymentCheck.Domain.Configuration
{

    public class EmploymentCheckConfiguration : IConfiguration, IEmploymentCheckConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }

        public CompaniesHouseConfiguration CompaniesHouse { get; set; }

        public string SubmissionEventApiAddress { get; set; }
    }
}
