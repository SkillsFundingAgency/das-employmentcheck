using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Configuration
{
    public class WhenAddingServicesToTheContainer
    {
        [TestCase(typeof(IEmployerAccountService))]
        [TestCase(typeof(IApiClient))]
        [TestCase(typeof(IEmployerAccountAuthorisationHandler))]
        [TestCase(typeof(IProviderAccountAuthorisationHandler))]
        [TestCase(typeof(IExternalAccountAuthorizationHandler))]
        [TestCase(typeof(IApiDescriptionHelper))]
        [TestCase(typeof(IUserService))]
        public void Then_The_Dependencies_Are_Correctly_Resolved(Type toResolve)
        {
            var serviceCollection = new ServiceCollection();
            SetupServiceCollection(serviceCollection);
            var provider = serviceCollection.BuildServiceProvider();

            var type = provider.GetService(toResolve);

            Assert.IsNotNull(type);
        }


        private static void SetupServiceCollection(ServiceCollection serviceCollection)
        {
            var configuration = GenerateConfiguration();

            serviceCollection.AddSingleton(Mock.Of<IWebHostEnvironment>());
            serviceCollection.AddSingleton(Mock.Of<IConfiguration>());
            serviceCollection.AddConfigurationOptions(configuration, AuthenticationType.Employer);
            serviceCollection.AddDistributedMemoryCache();
            serviceCollection.AddServiceRegistration(new ServiceParameters(), configuration);
            serviceCollection.AddMediatRValidation();
            serviceCollection.AddEmployerAuthenticationServices();
            serviceCollection.AddProviderAuthenticationServices();
            serviceCollection.AddExternalAuthenticationServices();
            serviceCollection.AddSharedAuthenticationServices();
        }

        private static IConfigurationRoot GenerateConfiguration()
        {
            var configSource = new MemoryConfigurationSource
            {
                InitialData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("EmployerApimDeveloperApi:BaseUrl", "https://test.com/"),
                    new KeyValuePair<string, string>("EmployerApimDeveloperApi:Key", "123edc"),
                    new KeyValuePair<string, string>("Environment", "test"),
                }
            };

            var provider = new MemoryConfigurationProvider(configSource);

            return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
        }
    }
}
