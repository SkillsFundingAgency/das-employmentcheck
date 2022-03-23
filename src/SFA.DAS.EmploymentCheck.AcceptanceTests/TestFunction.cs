using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.EmploymentCheck.Functions;
using SFA.DAS.EmploymentCheck.Functions.TestHelpers.AzureDurableFunctions;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

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

            var hmrcApiTokenServiceMock = new Mock<ITokenServiceApiClient>();
            hmrcApiTokenServiceMock.Setup(ts => ts.GetPrivilegedAccessTokenAsync())
                .ReturnsAsync(new PrivilegedAccessToken()
                {
                    AccessCode = "test_access_code",
                    ExpiryTime = DateTime.MaxValue
                });

            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            webHostEnvironmentMock.SetupGet(he => he.EnvironmentName).Returns("Development");

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
                            c.BaseUrl = testContext.DataCollectionsApi.BaseAddress;
                        });

                        s.Configure<ApplicationSettings>(a =>
                        {
                            a.DbConnectionString = testContext.SqlDatabase.DatabaseInfo.ConnectionString;
                            a.AllowedHashstringCharacters = "46789BCDFGHJKLMNPRSTVWXY";
                            a.Hashstring = "test hash string";
                        });

                        s.AddSingleton(typeof(IOrchestrationData), _orchestrationData);
                        s.AddSingleton(typeof(ITokenServiceApiClient), hmrcApiTokenServiceMock.Object);
                        s.AddSingleton(typeof(IWebHostEnvironment), webHostEnvironmentMock.Object);
                        
                        s.AddCommandServices(AddDecorators);

                        s.Decorate<IEventPublisher>((handler, sp) => new TestEventPublisher(handler, eventMessageHook));
                        s.Decorate<ICommandPublisher>((handler, sp) => new TestCommandPublisher(handler, commandMessageHook));
                        s.AddSingleton(commandMessageHook);
                    })
                )
                .UseEnvironment("LOCAL")
                .Build();
        }

        public IServiceCollection AddDecorators(IServiceCollection serviceCollection)
        {
            serviceCollection
                .Decorate(typeof(ICommandHandler<>), typeof(TestCommandHandlerReceived<>));
            
            serviceCollection
                .Decorate(typeof(ICommandHandler<>), typeof(TestCommandHandlerProcessed<>));

            return serviceCollection;
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

        public Task Start(OrchestrationStarterInfo starter, bool throwIfFailed = true)
        {
            return Jobs.Start(starter, throwIfFailed);
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

        public void ClearQueues()
        {
            using var process = new Process();
            process.StartInfo.FileName = "C:\\Program Files (x86)\\Microsoft SDKs\\Azure\\Storage Emulator\\AzureStorageEmulator.exe";
            process.StartInfo.Arguments = "clear queue";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
        }
    }
}