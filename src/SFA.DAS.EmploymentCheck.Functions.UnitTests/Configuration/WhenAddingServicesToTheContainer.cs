﻿using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetNiNumber;
using SFA.DAS.HashingService;
using SFA.DAS.TokenService.Api.Client;
using System;
using SFA.DAS.EmploymentCheck.Abstractions;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Configuration
{
    public class WhenAddingServicesToTheContainer
    {
        private Fixture _fixture = new Fixture();
        private ServiceProvider _provider;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Arrange
            _fixture = new Fixture();
            var serviceCollection = new ServiceCollection();
            SetupServiceCollection(serviceCollection);
            _provider = serviceCollection.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _provider.Dispose();
        }

        [TestCase(typeof(IHmrcApiOptionsRepository))]
        [TestCase(typeof(IHmrcApiRetryPolicies))]
        [TestCase(typeof(IDcTokenService))]
        [TestCase(typeof(IEmploymentCheckService))]
        [TestCase(typeof(ILearnerService))]
        [TestCase(typeof(IAzureClientCredentialHelper))]
        [TestCase(typeof(IEmployerAccountService))]
        [TestCase(typeof(IHmrcService))]
        [TestCase(typeof(IEmploymentCheckRepository))]
        [TestCase(typeof(IDataCollectionsResponseRepository))]
        [TestCase(typeof(IAccountsResponseRepository))]
        [TestCase(typeof(IEmploymentCheckCacheRequestRepository))]
        [TestCase(typeof(IHashingService))]
        [TestCase(typeof(ITokenServiceApiClient))]
        [TestCase(typeof(ICommandDispatcher))]
        [TestCase(typeof(ICommandHandler<CreateEmploymentCheckCacheRequestCommand>))]
        [TestCase(typeof(IQueryDispatcher))]
        [TestCase(typeof(IQueryHandler<GetNiNumberQueryRequest, GetNiNumberQueryResult>))]
        public void Then_The_Dependencies_Are_Correctly_Resolved(Type toResolve)
        {
            // Act
            var type = _provider.GetService(toResolve);

            // Assert
            Assert.IsNotNull(type);
        }

        private void SetupServiceCollection(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(Mock.Of<IWebHostEnvironment>());
            serviceCollection.AddOptions();

            var applicationSettings = _fixture.Create<ApplicationSettings>();
            serviceCollection.AddSingleton(applicationSettings);
            serviceCollection.AddSingleton<IOptions<ApplicationSettings>>(new OptionsWrapper<ApplicationSettings>(applicationSettings));

            var hmrcApiSettings = _fixture.Build<HmrcApiConfiguration>()
                .With(x => x.BaseUrl, "https://hostname.co")
                .Create();
            serviceCollection.AddSingleton<IOptions<HmrcApiConfiguration>>(new OptionsWrapper<HmrcApiConfiguration>(hmrcApiSettings));
            
            var apiConfiguration = _fixture.Build<EmployerAccountApiConfiguration>()
                .With(x => x.Url, "https://hostname.co")
                .Create();
            serviceCollection.AddSingleton(apiConfiguration);

            var dataCollectionsApiConfiguration = _fixture.Build<DataCollectionsApiConfiguration>()
                .With(x => x.Url, "https://hostname.co")
                .Create();
            serviceCollection.AddSingleton(dataCollectionsApiConfiguration);

            serviceCollection.AddEmploymentCheckService("PROD")
                .AddPersistenceServices()
                .AddNLog()
                .AddApprenticeshipLevyApiClient()
                .AddHashingService()
                .AddCommandServices()
                .AddQueryServices()
                ;
        }
    }
}
