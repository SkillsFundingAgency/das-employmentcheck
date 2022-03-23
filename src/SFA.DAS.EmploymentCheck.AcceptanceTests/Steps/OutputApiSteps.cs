using System;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;
using SFA.DAS.EmploymentCheck.Domain.Enums;
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
        private Data.Models.EmploymentCheck _check;
        private PublishEmploymentCheckResultCommand _publishedCommand;

        public OutputApiSteps(TestContext context) : base(context)
        {
            _context = context;
        }

        [Given(@"Completed employment check")]
        public async Task GivenCompletedEmploymentCheck()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            _check = _context.Fixture.Build<Data.Models.EmploymentCheck>()
                .Without(c => c.MessageSentDate)
                .With(c => c.RequestCompletionStatus, (short?)ProcessingCompletionStatus.Completed)
                .Create();

            await dbConnection.InsertAsync(_check);
        }

        [When(@"the output orchestrator is called")]
        public async Task WhenTheOutputOrchestratorIsCalled()
        {
            await _context.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "ResponseEmploymentChecksHttpTrigger",
                    args: new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                            { Path = "/api/orchestrators/ResponseOrchestrator" }
                    },
                    orchestrationName: nameof(ResponseOrchestrator)
                ),
                false);
        }

        [Then(@"the message with completed employment check is published")]
        public void ThenTheMessageWithCompletedEmploymentCheckIsPublished()
        {
            var publishedCommands = _context
                .CommandsPublished
                .Where(c => c.IsPublished &&
                            c.Command is PublishEmploymentCheckResultCommand).ToList();

            publishedCommands.Should().HaveCount(1);

            _publishedCommand = (PublishEmploymentCheckResultCommand)publishedCommands.First().Command;

            _publishedCommand.EmploymentCheck.Should().BeEquivalentTo(_check,
                opts => opts.Excluding(check => check.MessageSentDate));
        }

        [Then(@"the employment check record is marked as sent")]
        public void ThenTheEmploymentCheckRecordIsMarkedAsSent()
        {
            _publishedCommand.EmploymentCheck.MessageSentDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }
    }
}
