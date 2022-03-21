using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public static class WebHostBuilderExtensions
    {
        public static void UseNServiceBusContainer(this IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IServiceProviderFactory<UpdateableServiceProvider>>(new NServiceBusServiceProviderFactory());
        }
    }
}