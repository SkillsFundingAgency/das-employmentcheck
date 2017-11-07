using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using StructureMap;
using SFA.DAS.EmploymentCheck.Domain;
using SFA.DAS.EmploymentCheck.Domain.Configuration;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using StructureMap.TypeRules;
using IConfiguration = SFA.DAS.EmploymentCheck.Domain.Interfaces.IConfiguration;

namespace SubmissionEventWorkerRole.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        private string ServiceName = CloudConfigurationManager.GetSetting("ServiceName");
        private const string Version = "1.0";

        public DefaultRegistry()
        {
            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.ToUpperInvariant().StartsWith("SFA.DAS."));
                scan.RegisterConcreteTypesAgainstTheFirstInterface();

                var config = GetConfiguration();

                For<IConfiguration>().Use<EmploymentCheckConfiguration>();
                RegisterRepositories(config.DatabaseConnectionString);
                RegisterMapper();
                AddMediatrRegistrations();

                ConfigureLogging();
            });
        }
        private void ConfigureLogging()
        {
            For<ILog>().Use(x => new NLogLogger(x.ParentType, null, null)).AlwaysUnique();
        }

        private EmploymentCheckConfiguration GetConfiguration()
        {
            var environment = CloudConfigurationManager.GetSetting("EnvironmentName");

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository, new ConfigurationOptions(ServiceName, environment, Version));

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

        private void RegisterMapper()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("SFA.DAS.EmploymentCheck"));

            var mappingProfiles = new List<Profile>();

            foreach (var assembly in assemblies)
            {
                var profiles = Assembly.Load(assembly.FullName).GetTypes()
                    .Where(t => typeof(Profile).IsAssignableFrom(t))
                    .Where(t => t.IsConcrete() && t.HasConstructors())
                    .Select(t => (Profile)Activator.CreateInstance(t));

                mappingProfiles.AddRange(profiles);
            }

            var config = new MapperConfiguration(cfg =>
            {
                mappingProfiles.ForEach(cfg.AddProfile);
            });

            var mapper = config.CreateMapper();

            For<IConfigurationProvider>().Use(config).Singleton();
            For<IMapper>().Use(mapper).Singleton();
        }

        private void RegisterRepositories(string connectionString)
        {
            For<ISubmissionEventRepository>().Use<ISubmissionEventRepository>().Ctor<string>().Is(connectionString);
        }

    }
}
