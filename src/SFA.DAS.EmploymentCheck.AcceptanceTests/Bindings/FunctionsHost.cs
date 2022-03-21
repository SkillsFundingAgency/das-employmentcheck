﻿using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks;
using SFA.DAS.EmploymentCheck.Commands;
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

        [BeforeScenario]
        public async Task InitialiseHost()
        {
            var commandsHook = new Hook<ICommand>();
            _testContext.Hooks.Add(commandsHook);

            var eventsHook = new Hook<object>();
            _testContext.Hooks.Add(eventsHook);

            _testContext.TestFunction = new TestFunction(_testContext, $"TEST{_featureContext.FeatureInfo.Title}", eventsHook, commandsHook);
            _testContext.TestFunction.ClearQueues();
            await _testContext.TestFunction.StartHost();
        }

        [AfterScenario]
        public async Task Cleanup()
        { 
            await _testContext.TestFunction.DisposeAsync();
        }
    }
}
