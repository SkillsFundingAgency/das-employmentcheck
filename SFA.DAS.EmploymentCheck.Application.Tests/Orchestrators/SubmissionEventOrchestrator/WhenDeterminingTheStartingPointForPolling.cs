using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Queries.GetLastKnownProcessedSubmissionEvent;

namespace SFA.DAS.EmploymentCheck.Application.Tests.Orchestrators.SubmissionEventOrchestrator
{
    [TestFixture]
    public class WhenDeterminingTheStartingPointForPolling
    {
        private Application.Orchestrators.SubmissionEventOrchestrator _orchestrator;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            
            _orchestrator = new Application.Orchestrators.SubmissionEventOrchestrator(_mediator.Object);
        }

        private void SetUpValueOfResponse(long expectedValue)
        {
            _mediator.Setup(o => o.SendAsync(It.IsAny<GetLastKnownProcessedSubmissionEventRequest>())).ReturnsAsync(
                new GetLastKnownProcessedSubmissionEventResponse
                {
                    EventId = expectedValue
                });
        }

        [Test]
        public async Task ThenTheGetLastKnownProcessedSubmissionEventRequestIsInitiated()
        {
            SetUpValueOfResponse(15);

            await _orchestrator.GetLastKnownProcessedSubmissionEventId();

            _mediator.Verify(o => o.SendAsync(It.IsAny<GetLastKnownProcessedSubmissionEventRequest>()), Times.Exactly(1));
        }

        [Test]
        public async Task ThenTheGetLastKnownProcessedSubmissionEventResponseIsProcessed()
        {
            SetUpValueOfResponse(15);

            var response = await _orchestrator.GetLastKnownProcessedSubmissionEventId();

            Assert.IsNotNull(response);
            Assert.AreEqual(15, response.Data);
        }
    }
}
