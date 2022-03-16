using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.NServiceBus.AzureFunction.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using System;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection, Func<IServiceCollection, IServiceCollection> addDecorators = null)
        {
            serviceCollection.AddScoped<ICommandDispatcher, CommandDispatcher>();

            serviceCollection.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();
            });

            serviceCollection.AddScoped<ICommandPublisher, CommandPublisher>();

            if (addDecorators != null)
            {
                serviceCollection = addDecorators(serviceCollection);
            }

            return serviceCollection;
        }

        public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            ILogger logger,
            Action<NServiceBusOptions> onConfigureOptions = null)
        {
            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            var options = new NServiceBusOptions
            {
                OnMessageReceived = (context) =>
                {
                    context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out string messageType);
                    context.Headers.TryGetValue("NServiceBus.MessageId", out string messageId);
                    context.Headers.TryGetValue("NServiceBus.CorrelationId", out string correlationId);
                    context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out string originatingEndpoint);
                    logger.LogDebug($"Received NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");

                },
                OnMessageErrored = (ex, context) =>
                {
                    context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out string messageType);
                    context.Headers.TryGetValue("NServiceBus.MessageId", out string messageId);
                    context.Headers.TryGetValue("NServiceBus.CorrelationId", out string correlationId);
                    context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out string originatingEndpoint);
                    logger.LogError(ex, $"Error handling NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");
                }
            };

            onConfigureOptions?.Invoke(options);

            webBuilder.AddExtension(new NServiceBusExtensionConfigProvider(options));

            return serviceCollection;
        }
    }
}
