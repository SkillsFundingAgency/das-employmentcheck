using Microsoft.Azure.WebJobs;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.NServiceBus.AzureFunction.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
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

        public static void StartNServiceBus(this UpdateableServiceProvider serviceProvider,
            IConfiguration configuration, bool configurationIsLocalOrDev)
        {
            var endpointName = configuration["ApplicationSettings:NServiceBusEndpointName"];
            if (string.IsNullOrEmpty(endpointName))
            {
                endpointName = "SFA.DAS.EmploymentCheck.DomainMessageHandlers";
            }

            var endpointConfiguration = new EndpointConfiguration(endpointName)
                .UseErrorQueue()
                .UseInstallers()
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseOutbox(true)
                .UseServicesBuilder(serviceProvider)
                .UseSqlServerPersistence(() => new SqlConnection(configuration["DbConnectionString"]))
                .UseUnitOfWork()
                .UseSendOnly();

            if (configurationIsLocalOrDev)
            {
                endpointConfiguration.UseLearningTransport();
            }
            else
            {
                endpointConfiguration.UseAzureServiceBusTransport(configuration["NServiceBusConnectionString"], r => r.AddRouting());
            }

            var license = configuration["NServiceBusLicense"];
            if (!string.IsNullOrEmpty(license))
            {
                endpointConfiguration.License(license);
            }

            var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            serviceProvider.AddSingleton(p => endpoint)
                .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>())
                .AddHostedService<NServiceBusHostedService>();
        }
    }
}
