using FluentAssertions;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using SFA.DAS.EmploymentCheck.Functions.TestHelpers.AzureDurableFunctions;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "Singleton")]
    public class SingletonSteps
    {
        private readonly TestContext _context;
        private HttpResponseMessage _result;

        public SingletonSteps(TestContext context)
        {
            _context = context;
        }

        [Given(@"a running instance of Employment Check orchestrator")]
        public async Task GivenARunningInstanceOfEmploymentCheckOrchestrator()
        {
            await _context.TestFunction.CallEndpoint(
                new EndpointInfo(
                    starterName: nameof(EmploymentChecksHttpTrigger),
                    args: new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest { Path = "/api/orchestrators/EmploymentChecksOrchestrator" }
                    }
                ));
        }

        [When(@"the orchestrator is triggered")]
        public async Task WhenTheOrchestratorIsTriggered()
        {
            _result = await _context.TestFunction.CallEndpoint(
                new EndpointInfo(
                    starterName: nameof(EmploymentChecksHttpTrigger),
                    args: new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest { Path = "/api/orchestrators/EmploymentChecksOrchestrator" }
                    }
                ));
        }

        [Then(@"an error response is returned")]
        public void ThenAnErrorResponseIsReturned()
        {
            _result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}
