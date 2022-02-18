using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Bindings
{
    [Binding]
    public class FunctionsHost
    {
        private readonly TestContext _testContext;
        private readonly FeatureContext _featureContext;
        public FunctionsHost(TestContext testContext, FeatureContext featureContext)
        {
            _testContext = testContext;
            _featureContext = featureContext;
        }

        [BeforeScenario()]
        public async Task InitialiseHost()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _testContext.TestFunction = new TestFunction(_testContext, $"TEST{_featureContext.FeatureInfo.Title}");
            await _testContext.TestFunction.StartHost();
            stopwatch.Stop();
            Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
        }

        [AfterScenario()]
        public async Task Cleanup()
        { 
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await _testContext.TestFunction.DisposeAsync();
            stopwatch.Stop();
            Console.WriteLine($"Time it took to Cleanup  FunctionsHost: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
        }
    }
}
