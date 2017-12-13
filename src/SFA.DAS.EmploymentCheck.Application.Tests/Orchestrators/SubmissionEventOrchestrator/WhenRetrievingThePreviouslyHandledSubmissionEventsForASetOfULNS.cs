using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Queries.GetLastKnownProcessedSubmissionEvent;
using SFA.DAS.EmploymentCheck.Application.Queries.GetPreviouslyHandledSubmissionEventsFor;
using SFA.DAS.EmploymentCheck.Domain.Models;

namespace SFA.DAS.EmploymentCheck.Application.Tests.Orchestrators.SubmissionEventOrchestrator
{
    [TestFixture]
    public class WhenRetrievingThePreviouslyHandledSubmissionEventsForASetOfULNS
    {
        private Application.Orchestrators.SubmissionEventOrchestrator _orchestrator;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();

            _mediator.Setup(o => o.SendAsync(It.IsAny<GetPreviouslyHandledSubmissionEventsForEventRequest>())).ReturnsAsync(
                new GetPreviouslyHandledSubmissionEventsForEventResponse
                {
                    PreviouslyHandledSubmissionEvents = new List<PreviousHandledSubmissionEvent>
                    {
                        new PreviousHandledSubmissionEvent
                        {
                            Uln = "12234"
                        }
                    }
                });

            _orchestrator = new Application.Orchestrators.SubmissionEventOrchestrator(_mediator.Object);
        }

        [Test]
        public async Task ThenTheGetPreviousHandledSubmissionEventsForAGivenListOfUlnsIsInitiated()
        {
            await _orchestrator.GetPreviouslyHandledSubmissionEvents("");

            _mediator.Verify(o => o.SendAsync(It.IsAny<GetPreviouslyHandledSubmissionEventsForEventRequest>()), Times.Once);
        }

        [Test]
        public async Task ThenTheGetPreviouslyHandledSubmissionEventsResponseIsProcessed()
        {
            var response = await _orchestrator.GetPreviouslyHandledSubmissionEvents("");

            Assert.IsTrue(response.Data.Events.Any());
            Assert.AreEqual(response.Data.Events.ElementAt(0).Uln, "12234");
        }
    }
}
