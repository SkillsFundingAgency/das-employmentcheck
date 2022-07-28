using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks;
using SFA.DAS.EmploymentCheck.Functions;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using SFA.DAS.EmploymentCheck.Functions.TestHelpers.AzureDurableFunctions;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class TestFunction : IDisposable
    {
        private readonly IHost _host;
        private readonly OrchestrationData _orchestrationData;
        private bool _isDisposed;
        private IJobHost Jobs => _host.Services.GetService<IJobHost>();
        public string HubName { get; }
        public HttpResponseMessage LastResponse => ResponseObject as HttpResponseMessage;
        public object ResponseObject { get; private set; }

        public TestFunction(TestContext testContext, string hubName, IHook<object> eventMessageHook, IHook<ICommand> commandMessageHook)
        {
            HubName = hubName;
            _orchestrationData = new OrchestrationData();

            var appConfig = new Dictionary<string, string>
            {
                { "EnvironmentName", "LOCAL_ACCEPTANCE_TESTS" },
                { "AzureWebJobsStorage", "UseDevelopmentStorage=true" },
                { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" },
                { "ConfigNames", "SFA.DAS.EmploymentCheck.Functions" },
                { "ApplicationSettings:LogLevel", "DEBUG" },
                { "ApplicationSettings:NServiceBusConnectionString", "UseLearningEndpoint=true" },
                { "ApplicationSettings:UseLearningEndpointStorageDirectory", Path.Combine(testContext.TestDirectory.FullName, ".learningtransport") },
                { "ApplicationSettings:DbConnectionString", testContext.SqlDatabase.DatabaseInfo.ConnectionString },
                { "ApplicationSettings:NServiceBusEndpointName", testContext.InstanceId },
            };

            _host = new HostBuilder()
                .ConfigureAppConfiguration(a =>
                {
                    a.Sources.Clear();
                    a.AddInMemoryCollection(appConfig);
                })
                .ConfigureWebJobs(builder => builder
                    .AddHttp(options => options.SetResponse = (request, o) => { ResponseObject = o; })
                    .AddDurableTask(options =>
                    {
                        options.HubName = HubName;
                        options.UseAppLease = false;
                        options.UseGracefulShutdown = false;
                        options.ExtendedSessionsEnabled = false;
                        options.StorageProvider["maxQueuePollingInterval"] = new TimeSpan(0, 0, 0, 0, 500);
                        options.StorageProvider["partitionCount"] = 1;
                        options.NotificationUrl = new Uri("localhost:7071");

                    })
                    .AddAzureStorageCoreServices()
                    .ConfigureServices(s =>
                    {
                        new Startup().Configure(builder);

                        s.Configure<EmployerAccountApiConfiguration>(l =>
                        {
                            l.Url = testContext.EmployerAccountsApi.BaseAddress;
                            l.Identifier = "";
                        });

                        s.Configure<HmrcApiConfiguration>(c =>
                        {
                            c.BaseUrl = testContext.HmrcApi.BaseAddress;
                        });

                        s.Configure<DataCollectionsApiConfiguration>(c =>
                        {
                            c.BaseUrl = testContext.DataCollectionsApiConfiguration.BaseUrl;
                            c.Path = testContext.DataCollectionsApiConfiguration.Path;
                        });

                        s.Configure<ApplicationSettings>(a =>
                        {
                            a.DbConnectionString = testContext.SqlDatabase.DatabaseInfo.ConnectionString;
                        });

                        s.AddSingleton(typeof(IDcTokenService), CreateDcTokenServiceMock().Object);
                        s.AddSingleton(typeof(IOrchestrationData), _orchestrationData);
                        s.AddSingleton(typeof(ITokenServiceApiClient), CreateHmrcApiTokenServiceMock().Object);
                        s.AddSingleton(typeof(IWebHostEnvironment), CreateWebHostEnvironmentMock().Object);                        
                        s.AddSingleton(typeof(IHmrcApiOptionsRepository), CreateHmrcApiOptionsRepository().Object);
                        s.AddSingleton(typeof(IApiOptionsRepository), CreateApiOptionsRepository().Object);
                        s.Decorate<IEventPublisher>((handler, sp) => new TestEventPublisher(handler, eventMessageHook));
                        s.AddSingleton(commandMessageHook);
                    })
                )
                .UseEnvironment("LOCAL")
                .Build();
        }

        private static Mock<IWebHostEnvironment> CreateWebHostEnvironmentMock()
        {
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            webHostEnvironmentMock.SetupGet(he => he.EnvironmentName).Returns("Development");
            return webHostEnvironmentMock;
        }

        private static Mock<ITokenServiceApiClient> CreateHmrcApiTokenServiceMock()
        {
            var mock = new Mock<ITokenServiceApiClient>();
            mock.Setup(_ => _.GetPrivilegedAccessTokenAsync())
                .ReturnsAsync(new PrivilegedAccessToken
                {
                    AccessCode = "test_access_code",
                    ExpiryTime = DateTime.MaxValue
                });
            return mock;
        }

        private static Mock<IDcTokenService> CreateDcTokenServiceMock()
        {
            var mock = new Mock<IDcTokenService>();
            mock.Setup(_ => _.GetTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthResult
                {
                    AccessToken = "test_access_token",
                    ExpiresIn = 999999,
                    ExtExpiresIn = 999999,
                    TokenType = "TokenType"

                });
            return mock;
        }

        private static Mock<IHmrcApiOptionsRepository> CreateHmrcApiOptionsRepository()
        {
            var mock = new Mock<IHmrcApiOptionsRepository>();
            mock.Setup(_ => _.GetHmrcRateLimiterOptions())
                .ReturnsAsync(new HmrcApiRateLimiterOptions
                {
                    DelayInMs = 0,
                    TokenFailureRetryDelayInMs = 0,
                    TransientErrorDelayInMs = 0
                });
            return mock;
        }

        private static Mock<IApiOptionsRepository> CreateApiOptionsRepository()
        {
            var mock = new Mock<IApiOptionsRepository>();
            mock.Setup(_ => _.GetOptions(It.IsAny<string>()))
                .ReturnsAsync(new ApiRetryOptions
                {
                    TransientErrorDelayInMs = 0
                });
            return mock;
        }

        public async Task ExecuteCreateEmploymentCheckCacheRequestsOrchestrator()
        {
            var response = await Start(
                new OrchestrationStarterInfo(
                    starterName: nameof(CreateEmploymentCheckRequestsOrchestratorHttpTrigger),
                    orchestrationName: nameof(CreateEmploymentCheckCacheRequestsOrchestrator),
                    args: new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest { Path = "/api/orchestrators/CreateEmploymentCheckRequestsOrchestrator" }
                    }
                ));

            response.EnsureSuccessStatusCode();
        }

        public async Task ExecuteProcessEmploymentCheckRequestsOrchestrator()
        {
            var response = await Start(
                new OrchestrationStarterInfo(
                    starterName: nameof(ProcessEmploymentChecksOrchestratorHttpTrigger),
                    orchestrationName: nameof(ProcessEmploymentCheckRequestsOrchestrator),
                    args: new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest { Path = "/api/orchestrators/ProcessApprenticeEmploymentChecksOrchestrator" }
                    }
                ));

            response.EnsureSuccessStatusCode();
        }

        public async Task StartHost()
        {
            var timeout = new TimeSpan(0, 0, 10);
            var delayTask = Task.Delay(timeout);
            await Task.WhenAny(Task.WhenAll(_host.StartAsync(), Jobs.Terminate()), delayTask);

            if (delayTask.IsCompleted)
            {
                throw new Exception($"Failed to start test function host within {timeout.Seconds} seconds.  Check the AzureStorageEmulator is running. ");
            }
        }

        public async Task<HttpResponseMessage> Start(OrchestrationStarterInfo starter, bool throwIfFailed = true)
        {
            await Jobs.Start(starter, throwIfFailed);
            return ResponseObject as HttpResponseMessage;
        }

        public async Task<HttpResponseMessage> CallEndpoint(EndpointInfo endpoint)
        {
            await Jobs.Start(endpoint);
            return ResponseObject as HttpResponseMessage;
        }

        public async Task<OrchestratorStartResponse> GetOrchestratorStartResponse()
        {
            var responseString = await LastResponse.Content.ReadAsStringAsync();
            var responseValue = JsonConvert.DeserializeObject<OrchestratorStartResponse>(responseString);
            return responseValue;
        }

        public async Task<DurableOrchestrationStatus> GetStatus(string instanceId)
        {
            await Jobs.RefreshStatus(instanceId);
            return _orchestrationData.Status;
        }

        public async Task DisposeAsync()
        {
            await Jobs.StopAsync();
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _host.Dispose();
            }

            _isDisposed = true;
        }
    }
}