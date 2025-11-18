#nullable enable annotations
using System;
using System.Text.RegularExpressions;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Newtonsoft.Json;
using NServiceBus.Transport.AzureServiceBus;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public static class NServiceBusStartupExtensions
    {
        public static IServiceCollection AddNServiceBus(this IServiceCollection services, ApplicationSettings appSettings)
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmploymentCheck");

            var scanner = endpointConfiguration.AssemblyScanner();
            scanner.ExcludeAssemblies(
                "Azure.Core",
                "Microsoft.Bcl.AsyncInterfaces",
                "System.Threading.Tasks.Extensions",
                "System.Reactive.Core",
                "System.Reactive",
                "System.Reactive.Linq",
                "System.ClientModel",
                "Grpc.Core",
                "Grpc.Core.Api",
                "Grpc.Net.Client",
                "Grpc.Net.Common",
                "Google.Protobuf"
            );
            scanner.ThrowExceptions = false;
            scanner.ScanAppDomainAssemblies = true;

            var raw = appSettings.NServiceBusConnectionString?.Trim();

            if (string.Equals(raw, "UseLearningEndpoint=true", StringComparison.OrdinalIgnoreCase))
            {
                endpointConfiguration.UseTransport<LearningTransport>();
            }
            else
            {
                var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
                ConfigureAzureServiceBusTransport(transport, raw);
            }

            endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Commands"));
            conventions.DefiningEventsAs(t =>
                t.Namespace is not null &&
                t.Name.EndsWith("Event", StringComparison.Ordinal) &&
                (
                    t.Namespace.EndsWith(".Events", StringComparison.Ordinal) ||
                    t.Namespace.EndsWith(".Types", StringComparison.Ordinal)
                ));
            conventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages"));

            endpointConfiguration.EnableInstallers();
            endpointConfiguration.SendFailedMessagesTo("error");

            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            services.AddSingleton<IMessageSession>(endpointInstance);

            return services;
        }

        private static void ConfigureAzureServiceBusTransport(
            TransportExtensions<AzureServiceBusTransport> transport,
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(
                    "ApplicationSettings:NServiceBusConnectionString is required. " +
                    "Set 'UseLearningEndpoint=true' locally or 'Endpoint=sb://<ns>.servicebus.windows.net/' in Azure.");

            var hasKey = value.Contains("SharedAccessKey", StringComparison.OrdinalIgnoreCase);
            var hasSas = value.Contains("SharedAccessSignature", StringComparison.OrdinalIgnoreCase);

            var endpointMatch = Regex.Match(
                value,
                @"Endpoint=sb:\/\/(?<host>[^\/;]+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(250));

            if (endpointMatch.Success && !(hasKey || hasSas))
            {
                var fqdn = endpointMatch.Groups["host"].Value.Trim();
                transport.CustomTokenCredential(fqdn, new DefaultAzureCredential());
                return;
            }

            if (!value.Contains(';') && value.Contains(".servicebus.windows.net", StringComparison.OrdinalIgnoreCase))
            {
                var cleaned = value.Replace("sb://", "", StringComparison.OrdinalIgnoreCase).TrimEnd('/');
                transport.CustomTokenCredential(cleaned, new DefaultAzureCredential());
                return;
            }

            transport.ConnectionString(value);
        }
    }
}
