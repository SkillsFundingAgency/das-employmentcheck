using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Queries;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.ResetEmploymentChecksMessageSentDateOrchestratorTests
{
    public class WhenResetEmploymentChecksMessageSentDateOrchestratorIsTriggered
    {
        private ResetEmploymentChecksMessageSentDateOrchestrator _sut;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;

        [SetUp]
        public void SetUp()
        {
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _sut = new ResetEmploymentChecksMessageSentDateOrchestrator();
        }

        [Test]
        public async Task Then_ResetEmploymentChecksMessageSentDateActivity_Is_Called()
        {
            // Act
            await _sut.ResetEmploymentChecksMessageSentDateTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<long>(nameof(ResetEmploymentChecksMessageSentDateActivity), It.IsAny<IQueryDispatcher>()), Times.Once);
        }


        [Test]
        public async Task Then_Error_Is_Returned__in_case_of_exception()
        {
            // Arrange
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<long>(nameof(ResetEmploymentChecksMessageSentDateActivity), It.IsAny<IQueryDispatcher>()))
                .Throws(new Exception());

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDateTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<long>(nameof(ResetEmploymentChecksMessageSentDateActivity), It.IsAny<IQueryDispatcher>()), Times.Once);
            result.Should().Contain("Exception");
        }
    }
}
