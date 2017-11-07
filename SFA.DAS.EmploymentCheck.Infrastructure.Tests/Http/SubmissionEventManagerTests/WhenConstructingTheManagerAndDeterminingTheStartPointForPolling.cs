using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Domain;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Http;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Tests.Http.SubmissionEventManagerTests
{
    public class WhenConstructingTheManagerAndDeterminingTheStartPointForPolling { 
        private SubmissionEventManager _submissionEventManager;
        private Mock<IHttpClientWrapper> _httpClientWrapper;
        private Mock<ISubmissionEventOrchestrator> _orchestrator;
        private Mock<IEmploymentCheckConfiguration> _configuration;

        [SetUp]
        public void Arrange()
        {
            _httpClientWrapper = new Mock<IHttpClientWrapper>();
            _configuration = new Mock<IEmploymentCheckConfiguration>();
            _orchestrator = new Mock<ISubmissionEventOrchestrator>();
            _orchestrator.Setup(o => o.GetLastKnownProcessedSubmissionEventId())
                .Returns(Task.FromResult(new OrchestratorResponse<long>() { Data = 12 }));
            _submissionEventManager = new SubmissionEventManager(_httpClientWrapper.Object, _orchestrator.Object, _configuration.Object);
        }

        [Test]
        public void ThenWhenTheHttpClientWrapperIsNotProvidedDuringConstructionThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _submissionEventManager = new SubmissionEventManager(null, null, null);
            });
        }

        [Test]
        public void ThenWhenTheOrchestratorIsNotProvidedDuringConstructionThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _submissionEventManager = new SubmissionEventManager(_httpClientWrapper.Object, null, null);
            });
        }

        [Test]
        public void ThenWhenTheConfigurationIsNotProvidedDuringConstructionThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _submissionEventManager = new SubmissionEventManager(_httpClientWrapper.Object, _orchestrator.Object, null);
            });
        }

        [Test]
        public async Task ThenDuringConstructionTheLastKnownProcessedEventIdIsDeterminedFromTheDatabase()
        {
            await _submissionEventManager.DetermineProcessingStartingPoint();
            _orchestrator.Verify(o => o.GetLastKnownProcessedSubmissionEventId(), Times.Exactly(1));
         
        }

        [Test]
        public async Task ThenDuringPollingTheLastEventIdIsStoredInTheLastProcessedEventIdProperty()
        {
            await _submissionEventManager.DetermineProcessingStartingPoint();
            Assert.AreEqual(12, _submissionEventManager.LastProcessedEventId);
        }
    }
}
