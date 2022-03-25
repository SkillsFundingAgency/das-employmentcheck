using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using System;
using System.IO;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public static class NServiceBusStartupExtensions
    {
        private static EndpointConfiguration _endpointConfiguration;

        public static IServiceCollection AddNServiceBus(
           this IServiceCollection serviceCollection,
           ApplicationSettings configuration)
        {
            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            _endpointConfiguration = new EndpointConfiguration("sfa.das.employmentcheck")
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseSqlServerPersistence(() => new SqlConnection(configuration.DbConnectionString))
                .UseUnitOfWork();

            if (configuration.NServiceBusConnectionString.Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory()
                    .IndexOf("src", StringComparison.Ordinal)], "src\\.learningtransport");
                _endpointConfiguration
                    .UseTransport<LearningTransport>()
                    .StorageDirectory(dir);
                _endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            }
            else
            {
                _endpointConfiguration
                    .UseAzureServiceBusTransport(configuration.NServiceBusConnectionString, r => r.AddRouting());
            }

            if (!string.IsNullOrEmpty(configuration.NServiceBusLicense))
            {
                _endpointConfiguration.License(configuration.NServiceBusLicense);
            }

            var endpoint = EndpointWithExternallyManagedServiceProvider.Create(_endpointConfiguration, serviceCollection);
            endpoint.Start(new UpdateableServiceProvider(serviceCollection));
            serviceCollection.AddSingleton(p => endpoint.MessageSession.Value);

            return serviceCollection;
        }

        // TODO: delete when Employer Incentives have released a subscriber
        public static IServiceCollection AddNServiceBusMessageHandlers(
            this IServiceCollection serviceCollection,
            ILogger logger,
            Action<NServiceBusOptions> onConfigureOptions = null)
        {
            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            var options = new NServiceBusOptions
            {
                OnMessageReceived = context =>
                {
                    context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out var messageType);
                    context.Headers.TryGetValue("NServiceBus.MessageId", out var messageId);
                    context.Headers.TryGetValue("NServiceBus.CorrelationId", out var correlationId);
                    context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out var originatingEndpoint);
                    logger.LogDebug($"Received NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");
                },
                OnMessageErrored = (ex, context) =>
                {
                    context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out var messageType);
                    context.Headers.TryGetValue("NServiceBus.MessageId", out var messageId);
                    context.Headers.TryGetValue("NServiceBus.CorrelationId", out var correlationId);
                    context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out var originatingEndpoint);
                    logger.LogError(ex, $"Error handling NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");
                },
            };

            onConfigureOptions?.Invoke(options);

            webBuilder.AddExtension(new NServiceBusExtensionConfigProvider(options));

            return serviceCollection;
        }
    }
}
