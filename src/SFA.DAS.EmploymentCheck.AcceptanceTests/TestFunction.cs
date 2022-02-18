using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.AcceptanceTests.AzureDurableFunctions;
using SFA.DAS.EmploymentCheck.Functions;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class TestFunction : IDisposable
    {
        private readonly TestContext _testContext;
        private readonly Dictionary<string, string> _appConfig;
        private readonly IHost _host;
        private readonly OrchestrationData _orchestrationData;
        private bool isDisposed;

        private IJobHost Jobs => _host.Services.GetService<IJobHost>();
        public string HubName { get; }
        public HttpResponseMessage LastResponse => ResponseObject as HttpResponseMessage;
        public ObjectResult HttpObjectResult => ResponseObject as ObjectResult;
        public object ResponseObject { get; private set; }

        public TestFunction(TestContext testContext, string hubName)
        {
            HubName = hubName;
            _orchestrationData = new OrchestrationData();

            _appConfig = new Dictionary<string, string>
            {
                { "EnvironmentName", "LOCAL_ACCEPTANCE_TESTS" },
                { "AzureWebJobsStorage", "UseDevelopmentStorage=true" },
                { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" },
                { "ConfigNames", "SFA.DAS.EmployerIncentives" },
                { "ApplicationSettings:LogLevel", "DEBUG" }
            };

            _testContext = testContext;

            _host = new HostBuilder()
                .ConfigureAppConfiguration(a =>
                {
                    a.Sources.Clear();
                    a.AddInMemoryCollection(_appConfig);
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
                            l.Url = _testContext.EmployerAccountsApi.BaseAddress;
                            l.Identifier = "";
                        });

                        s.Configure<HmrcApiConfiguration>(c => { c.BaseUrl = _testContext.HmrcApi.BaseAddress; });

                        s.Configure<ApplicationSettings>(a =>
                        {
                            a.DbConnectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;
                            a.AllowedHashstringCharacters = "46789BCDFGHJKLMNPRSTVWXY";
                            a.Hashstring = "test hash string";
                        });

                        s.AddSingleton(typeof(IOrchestrationData), _orchestrationData);
                    })
                )
                .Build();
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

        public async Task<ObjectResult> CallEndpoint(EndpointInfo endpoint)
        {
            await Jobs.Start(endpoint);
            return ResponseObject as ObjectResult;
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
            if (isDisposed) return;

            if (disposing)
            {
                _host.Dispose();
            }

            isDisposed = true;
        }
    }
}