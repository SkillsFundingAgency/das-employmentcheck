using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.ProcessEmploymentCheckRequestsWithRateLimiterOrchestratorTests
{
    public class WhenRunningProcessEmploymentCheckRequestsWithRateLimiterOrchestrator
    {
        private readonly Fixture _fixture;
        private readonly Mock<IDurableOrchestrationContext> _context;
        private readonly Mock<ILogger<ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator>> _logger;
        private readonly IList<Models.EmploymentCheck> _employmentChecks;
        private readonly IList<LearnerNiNumber> _learnerNiNumbers;
        private readonly IList<EmployerPayeSchemes> _employerPayeSchemes;
        private readonly EmploymentCheckCacheRequest _employmentCheckCacheRequest;

        public WhenRunningProcessEmploymentCheckRequestsWithRateLimiterOrchestrator()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator>>();
            _employmentChecks = new List<Models.EmploymentCheck> { _fixture.Create<Models.EmploymentCheck>() };
            _learnerNiNumbers = new List<LearnerNiNumber> { _fixture.Create<LearnerNiNumber>() };
            _employerPayeSchemes = new List<EmployerPayeSchemes> { _fixture.Create<EmployerPayeSchemes>() };
            _employmentCheckCacheRequest = _fixture.Build<EmploymentCheckCacheRequest>().With(e => e.Employed, true).Create();
        }

        [Test]
        public async Task Then_The_Activities_Are_Called()
        {
            // Arrange
            var sut = new ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator(_logger.Object);

            _context.Setup(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), It.IsAny<object>()))
                    .ReturnsAsync(_employmentCheckCacheRequest);
            _context.Setup(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), It.IsAny<EmploymentCheckCacheRequest>()))
                .ReturnsAsync(_employmentCheckCacheRequest);

            _context.Setup(a => a.CallActivityAsync(nameof(StoreEmploymentCheckResultActivity), It.IsAny<EmploymentCheckCacheRequest>()));
            _context.Setup(a => a.CallActivityAsync<IList<EmploymentCheckCacheRequest>>(nameof(SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusActivity), It.IsAny<Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>>()));
            _context.Setup(a => a.CallActivityAsync<TimeSpan>(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity), It.IsAny<EmploymentCheckCacheRequest>()));

            // Act
            await sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), It.IsAny<object>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), It.IsAny<EmploymentCheckCacheRequest>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync(nameof(StoreEmploymentCheckResultActivity), It.IsAny<EmploymentCheckCacheRequest>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync<IList<EmploymentCheckCacheRequest>>(nameof(SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusActivity), It.IsAny<Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync<TimeSpan>(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity), It.IsAny<EmploymentCheckCacheRequest>()), Times.Once);
        }
    }
}