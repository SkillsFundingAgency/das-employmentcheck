using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Queries.GetLastKnownProcessedSubmissionEvent;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.Tests.Queries
{
    [TestFixture]
    public class WhenHandlingTheGetLastKnownProcessedSubmissionEventRequest
    {
        private GetLastKnownProcessedSubmissionEventHandler _handler;
        private Mock<ISubmissionEventRepository> _repository;

        [SetUp]
        public void Arrange()
        {
            _repository = new Mock<ISubmissionEventRepository>();
            _handler = new GetLastKnownProcessedSubmissionEventHandler(_repository.Object);
        }

        [Test]
        public void ThenThrowsAnArgumentNullExceptionIfTheRepositoryIsNotInstantiated()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _handler = new GetLastKnownProcessedSubmissionEventHandler(null);
            });
        }

        [Test]
        public async Task ThenRetrievesTheLastKnownProcessedEventIdFromTheRepository()
        {
            await _handler.Handle(new GetLastKnownProcessedSubmissionEventRequest());

            _repository.Verify(o => o.GetPollingProcessingStartingPoint(), Times.Once);
        }

        [Test]
        public async Task ThenReturnsTheLastKnownProcessedEventId()
        {
            _repository.Setup(o => o.GetPollingProcessingStartingPoint()).ReturnsAsync(2500);
            var actual = await _handler.Handle(new GetLastKnownProcessedSubmissionEventRequest());

            Assert.AreEqual(2500, actual.EventId);
        }
    }
}
