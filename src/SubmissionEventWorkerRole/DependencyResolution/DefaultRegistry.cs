using System;
using System.Net.Http;
using HMRC.ESFA.Levy.Api.Client;
using MediatR;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmploymentCheck.DataAccess;
using SFA.DAS.EmploymentCheck.Domain.Configuration;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.TokenService.Api.Client;
using StructureMap;
using TokenServiceApiClientConfiguration = SFA.DAS.EmploymentCheck.Domain.Configuration.TokenServiceApiClientConfiguration;

namespace SFA.DAS.EmploymentCheck.SubmissionEventWorkerRole.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        private readonly string _serviceName = CloudConfigurationManager.GetSetting("ServiceName");
        private readonly string _tokenServiceName = CloudConfigurationManager.GetSetting("TokenServiceName");
        private readonly string _serviceVersion = CloudConfigurationManager.GetSetting("ServiceVersion");

        public DefaultRegistry()
        {
            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.ToUpperInvariant().StartsWith("SFA.DAS."));
                scan.RegisterConcreteTypesAgainstTheFirstInterface();
                scan.AddAllTypesOf<IMessageProcessor>();
            });

            var employmentCheckConfig = GetConfiguration<EmploymentCheckConfiguration>(_serviceName);
            For<IEmploymentCheckConfiguration>().Use(employmentCheckConfig);

            var tokenServiceConfig = GetConfiguration<TokenServiceApiClientConfiguration>(_tokenServiceName);
            For<ITokenServiceApiClientConfiguration>().Use(tokenServiceConfig);

            RegisterRepositories(employmentCheckConfig.DatabaseConnectionString);
            RegisterApis(employmentCheckConfig);
            AddMediatrRegistrations();

            ConfigureLogging();
        }

        private void RegisterApis(EmploymentCheckConfiguration config)
        {
            For<IEventsApi>().Use(new EventsApi(config.EventsApi));
            For<IPaymentsEventsApiClient>().Use(new PaymentsEventsApiClient(config.PaymentsEvents));
            For<IApprenticeshipLevyApiClient>().Use(new ApprenticeshipLevyApiClient(GetLevyHttpClient(config)));
        }

        private HttpClient GetLevyHttpClient(EmploymentCheckConfiguration config)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(config.HmrcBaseUrl);
            return client;
        }

        private void ConfigureLogging()
        {
            For<ILog>().Use(x => new NLogLogger(x.ParentType, null, null)).AlwaysUnique();
        }

        private T GetConfiguration<T>(string serviceName)
        {
            var environment = CloudConfigurationManager.GetSetting("EnvironmentName");

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository, new ConfigurationOptions(serviceName, environment, _serviceVersion));

            return configurationService.Get<T>();
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
