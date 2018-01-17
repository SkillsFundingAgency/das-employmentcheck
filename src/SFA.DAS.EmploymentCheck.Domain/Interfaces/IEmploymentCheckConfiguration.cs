using SFA.DAS.EmploymentCheck.Domain.Configuration;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.EmploymentCheck.Domain.Interfaces
{
    public interface IEmploymentCheckConfiguration
    {
        string DatabaseConnectionString { get; set; }
        string MessageServiceBusConnectionString { get; set; }
        EventsApiClientConfiguration EventsApi { get; set; }
        PaymentsEventsApiConfiguration PaymentsEvents { get; set; }
        string HmrcBaseUrl { get; set; }
    }
}
