using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetNiNumber;
using SFA.DAS.EmploymentCheck.TokenServiceStub;
using SFA.DAS.TokenService.Api.Client;
using System;
using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Abstractions;
using System.Reflection;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.Encoding;
using SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Configuration
{
    public class WhenAddingServicesToTheContainer
    {
        private Fixture _fixture = new Fixture();
        private ServiceProvider _provider;
        private Infrastructure.Configuration.TokenServiceApiClientConfiguration _tokenConfig;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _fixture = new Fixture();
            _tokenConfig = _fixture.Build<Infrastructure.Configuration.TokenServiceApiClientConfiguration>().Create();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _provider.Dispose();
        }

        [TestCase(typeof(IHmrcApiOptionsRepository))]
        [TestCase(typeof(IHmrcApiRetryPolicies))]
        [TestCase(typeof(IApiRetryPolicies))]
        [TestCase(typeof(IDcTokenService))]
        [TestCase(typeof(IEmploymentCheckService))]
        [TestCase(typeof(ILearnerService))]
        [TestCase(typeof(INationalInsuranceNumberService))]
        [TestCase(typeof(INationalInsuranceNumberYearsService))]
        [TestCase(typeof(IAzureClientCredentialHelper))]
        [TestCase(typeof(IEmployerAccountService))]
        [TestCase(typeof(IHmrcService))]
        [TestCase(typeof(ILearnerNiNumberValidator))]
        [TestCase(typeof(IEmployerPayeSchemesValidator))]
        [TestCase(typeof(IEmploymentCheckDataValidator))]
        [TestCase(typeof(IDataCollectionsResponseRepository))]
        [TestCase(typeof(IAccountsResponseRepository))]
        [TestCase(typeof(IEmploymentCheckCacheRequestRepository))]
        [TestCase(typeof(ITokenServiceApiClient))]
        [TestCase(typeof(ICommandDispatcher))]
        [TestCase(typeof(ICommandHandler<CreateEmploymentCheckCacheRequestCommand>))]
        [TestCase(typeof(IQueryDispatcher))]
        [TestCase(typeof(IQueryHandler<GetNiNumberQueryRequest, GetNiNumberQueryResult>))]
        public void Then_The_Dependencies_Are_Correctly_Resolved(Type toResolve)
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            SetupServiceCollection(serviceCollection, "PROD");
            _provider = serviceCollection.BuildServiceProvider();

            // Act
            var type = _provider.GetService(toResolve);

            // Assert
            Assert.IsNotNull(type);
        }

        [TestCase("LOCAL")]
        [TestCase("DEV")]
        [TestCase("TEST")]
        [TestCase("TEST2")]
        [TestCase("PP")]
        [TestCase("DEMO")]
        [TestCase("MO")]
        public void Then_The_TokenServiceApiClient_Stub_Is_Used_For_Non_Prod_Environments(string environment)
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            SetupServiceCollection(serviceCollection, environment);
            _provider = serviceCollection.BuildServiceProvider();

            // Act
            var type = _provider.GetService(typeof(ITokenServiceApiClient)) as TokenServiceApiClientStub;

            // Assert
            Assert.IsNotNull(type);
        }

        [TestCase("PROD")]
        public void Then_The_Real_TokenServiceApiClient_Is_Used_For_Prod_Environments(string environment)
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            SetupServiceCollection(serviceCollection, environment);
            _provider = serviceCollection.BuildServiceProvider();

            // Act
            TokenServiceApiClient tokenServiceApiClient = _provider.GetService(typeof(ITokenServiceApiClient)) as TokenServiceApiClient;

            // Assert
            tokenServiceApiClient.Should().NotBeNull();
            FieldInfo field = typeof(TokenServiceApiClient).GetField("_configuration", BindingFlags.NonPublic |
                                             BindingFlags.Instance);

            var config = field.GetValue(tokenServiceApiClient);
            config.Should().Be(_tokenConfig);

        }

        private void SetupServiceCollection(IServiceCollection serviceCollection, string environment)
        {
            serviceCollection.AddSingleton(Mock.Of<IWebHostEnvironment>());
            serviceCollection.AddOptions();
            serviceCollection.AddMemoryCache();

            var applicationSettings = _fixture.Create<ApplicationSettings>();
            serviceCollection.AddSingleton(applicationSettings);
            serviceCollection.AddSingleton<IOptions<ApplicationSettings>>(new OptionsWrapper<ApplicationSettings>(applicationSettings));

            var hmrcApiSettings = _fixture.Build<HmrcApiConfiguration>()
                .With(x => x.BaseUrl, "https://hostname.co")
                .Create();
            serviceCollection.AddSingleton<IOptions<HmrcApiConfiguration>>(new OptionsWrapper<HmrcApiConfiguration>(hmrcApiSettings));

            serviceCollection.AddSingleton<IOptions<Infrastructure.Configuration.TokenServiceApiClientConfiguration>>(new OptionsWrapper<Infrastructure.Configuration.TokenServiceApiClientConfiguration>(_tokenConfig));

            var apiConfiguration = _fixture.Build<EmployerAccountApiConfiguration>()
                .With(x => x.Url, "https://hostname.co")
                .Create();
            serviceCollection.AddSingleton(apiConfiguration);

            var dataCollectionsApiConfiguration = _fixture.Build<DataCollectionsApiConfiguration>()
                .With(x => x.BaseUrl, "https://hostname.co")
                .Create();
            serviceCollection.AddSingleton(dataCollectionsApiConfiguration);
            var encodingConfig = new EncodingConfig
            {
                Encodings = new List<Encoding.Encoding>
                {
                    new Encoding.Encoding { Alphabet = "ABC", EncodingType = "Test", MinHashLength = 6, Salt = "Salt" }
                }
            };
            serviceCollection.AddSingleton(encodingConfig);
            serviceCollection.AddSingleton<IEncodingService, EncodingService>();

            serviceCollection.AddEmploymentCheckService(environment)
                .AddPersistenceServices()
                .AddNLog(Environment.CurrentDirectory, "LOCAL")
                .AddApprenticeshipLevyApiClient()
                .AddCommandServices()
                .AddQueryServices()
                ;
        }
    }
}
