using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.Messaging.AzureServiceBus.StructureMap;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.EmploymentCheck.Domain.Configuration
{
    public class EmploymentCheckConfiguration : IEmploymentCheckConfiguration, ITopicMessagePublisherConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string MessageServiceBusConnectionString { get; set; }
        public EventsApiClientConfiguration EventsApi { get; set; }
        public PaymentsEventsApiConfiguration PaymentsEvents { get; set; }
    }
}
