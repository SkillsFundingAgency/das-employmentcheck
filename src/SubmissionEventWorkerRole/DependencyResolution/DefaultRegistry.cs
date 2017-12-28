using MediatR;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmploymentCheck.DataAccess;
using StructureMap;
using SFA.DAS.EmploymentCheck.Domain.Configuration;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;

namespace SubmissionEventWorkerRole.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        private readonly string _serviceName = CloudConfigurationManager.GetSetting("ServiceName");
        private const string Version = "1.0";

        public DefaultRegistry()
        {
            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.ToUpperInvariant().StartsWith("SFA.DAS."));
                scan.RegisterConcreteTypesAgainstTheFirstInterface();
            });

            var config = GetConfiguration();

            For<IEmploymentCheckConfiguration>().Use(config);
            RegisterRepositories(config.DatabaseConnectionString);
            RegisterApis(config);
            AddMediatrRegistrations();

            ConfigureLogging();
        }

        private void RegisterApis(EmploymentCheckConfiguration config)
        {
            For<IEventsApi>().Use(new EventsApi(config.EventsApi));
            For<IPaymentsEventsApiClient>().Use(new PaymentsEventsApiClient(config.PaymentsEvents));
        }

        private void ConfigureLogging()
        {
            For<ILog>().Use(x => new NLogLogger(x.ParentType, null, null)).AlwaysUnique();
        }

        private EmploymentCheckConfiguration GetConfiguration()
        {
            var environment = CloudConfigurationManager.GetSetting("EnvironmentName");

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository, new ConfigurationOptions(_serviceName, environment, Version));

            return configurationService.Get<EmploymentCheckConfiguration>();
        }

        private static IConfigurationRepository GetConfigurationRepository()
        {
            return new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
        }

        private void AddMediatrRegistrations()
        {
            For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));

            For<IMediator>().Use<Mediator>();
        }

        private void RegisterRepositories(string connectionString)
        {
            For<ISubmissionEventRepository>().Use<SubmissionEventRepository>().Ctor<string>().Is(connectionString);
        }

    }
}
