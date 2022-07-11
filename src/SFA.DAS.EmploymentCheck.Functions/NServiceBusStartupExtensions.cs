using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
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
        public static IServiceCollection AddNServiceBus(
           this IServiceCollection serviceCollection,
           ApplicationSettings configuration)
        {
            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmploymentCheck.Results")
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseOutbox(true)
                .UseSqlServerPersistence(() => new SqlConnection(configuration.DbConnectionString))
                .UseUnitOfWork();

            if (configuration.NServiceBusConnectionString.Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory()
                    .IndexOf("src", StringComparison.Ordinal)], "src\\.learningtransport");
                endpointConfiguration
                    .UseTransport<LearningTransport>()
                    .StorageDirectory(dir);
                endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            }
            else
            {
                endpointConfiguration
                    .UseAzureServiceBusTransport(configuration.NServiceBusConnectionString, r => r.AddRouting())
                    .UseEndpointWithExternallyManagedService(serviceCollection);
            }

            if (!string.IsNullOrEmpty(configuration.NServiceBusLicense))
            {
                endpointConfiguration.License(configuration.NServiceBusLicense);
            }

            return serviceCollection;
        }

        public static IServiceCollection AddNServiceBusMessageHandlers(
            this IServiceCollection serviceCollection,
            ILogger logger,
            ApplicationSettings configuration,
            Action<NServiceBusOptions> onConfigureOptions = null)
        {
            Environment.SetEnvironmentVariable("NServiceBusConnectionString", configuration.NServiceBusConnectionString, EnvironmentVariableTarget.Process);

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
