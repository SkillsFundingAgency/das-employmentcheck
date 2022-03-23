using FluentAssertions;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.TestHelpers.AzureDurableFunctions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "OutputApi")]
    public class OutputApiSteps : StepsBase
    {
        private readonly TestContext _context;

        public OutputApiSteps(TestContext context) : base(context)
        {
            _context = context;
        }

        [Given(@"Completed employment check")]
        public void GivenCompletedEmploymentCheck()
        {
            //
        }

        [When(@"the output orchestrator is called")]
        public async Task WhenTheOutputOrchestratorIsCalled()
        {
            await _context.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "OutputEmploymentCheckResultsStubHttpTrigger",
                    args: new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                            { Path = "/api/orchestrators/OutputEmploymentCheckResultsStubOrchestrator" }
                    },
                    orchestrationName: nameof(OutputEmploymentCheckResultsStubOrchestrator)
                ),
                false);
        }

        [Then(@"the message with completed employment check is published")]
        public void ThenTheMessageWithCompletedEmploymentCheckIsPublished()
        {
            _context
                .CommandsPublished
                .Where(c => c.IsPublished &&
                            c.Command is PublishEmploymentCheckResultCommand)
                .Should().HaveCount(1);
        }
    }
}
