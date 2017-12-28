using SFA.DAS.EmploymentCheck.Domain.Interfaces;

namespace SFA.DAS.EmploymentCheck.Domain.Configuration
{

    public class EmploymentCheckConfiguration : IEmploymentCheckConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
    }
}
