using System.Threading.Tasks;
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
            _testContext.TestFunction = new TestFunction(_testContext, $"TEST{_featureContext.FeatureInfo.Title}");
            await _testContext.TestFunction.StartHost();
        }

        [AfterScenario]
        public async Task Cleanup()
        { 
            await _testContext.TestFunction.DisposeAsync();
        }
    }
}
