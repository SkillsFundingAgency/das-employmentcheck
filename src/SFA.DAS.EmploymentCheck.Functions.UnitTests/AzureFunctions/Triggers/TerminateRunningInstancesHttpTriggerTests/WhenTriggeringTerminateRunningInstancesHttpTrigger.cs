using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Extensions;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.TerminateRunningInstancesHttpTriggerTests
{
    public class WhenTriggeringTerminateRunningInstancesHttpTrigger
    {
        private Mock<HttpRequestMessage> _request;
        private Mock<IDurableOrchestrationClient> _starter;
        private Mock<ILogger> _logger;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _request = new Mock<HttpRequestMessage>();
            _starter = new Mock<IDurableOrchestrationClient>();
            _logger = new Mock<ILogger>();
        }

        [Test]
        public async Task Then_all_running_instances_are_terminated()
        {
            // Arrange
            var instances = _fixture.Create<OrchestrationStatusQueryResult>();
            _starter.Setup(x => x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(
                    c => c.RuntimeStatus.SequenceEqual(DurableOrchestrationClientExtensions.RuntimeStatuses)
                ), CancellationToken.None)
            ).ReturnsAsync(instances);

            // Act
            await TerminateRunningInstancesHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            // Assert
            instances.DurableOrchestrationState.Should().HaveCount(3);
            foreach (var durableOrchestrationStatus in instances.DurableOrchestrationState)
            {
                _starter.Verify(s => s.TerminateAsync(durableOrchestrationStatus.InstanceId, It.IsAny<string>()), Times.Once);
            }
        }

        [Test]
        public async Task Then_the_number_of_terminated_instances_is_returned()
        {
            // Arrange
            var instances = _fixture.Create<OrchestrationStatusQueryResult>();
            _starter.Setup(x => x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(
                    c => c.RuntimeStatus.SequenceEqual(DurableOrchestrationClientExtensions.RuntimeStatuses)
                ), CancellationToken.None)
            ).ReturnsAsync(instances);

            // Act
            var result = await TerminateRunningInstancesHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            instances.DurableOrchestrationState.Should().HaveCount(3);
            result.Content.ReadAsStringAsync().Result.Should().Be("Terminated 3 orchestrator instances.");
        }
    }
}