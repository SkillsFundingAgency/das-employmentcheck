using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Extensions
{
    public class DurableOrchestrationClientExtensionsTests
    {
        private string _orchestratorName;
        private Mock<IDurableOrchestrationClient> _starter;
        private OrchestrationStatusQueryResult _instances;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _orchestratorName = _fixture.Create<string>();
            _starter = new Mock<IDurableOrchestrationClient>();
        }

        [Test]
        public async Task Does_Not_Start_Orchestrator_When_Finds_Existing_Instances()
        {
            // Arrange
            _instances = _fixture.Create<OrchestrationStatusQueryResult>();
            _starter.Setup(x => x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(
                    c =>
                        c.InstanceIdPrefix == _orchestratorName
                        && c.RuntimeStatus.SequenceEqual(DurableOrchestrationClientExtensions.RuntimeStatuses)
                ), CancellationToken.None)

            ).ReturnsAsync(_instances);

            // Act
            await _starter.Object.StartIfNotRunning(_orchestratorName);

            // Assert
            _starter.Verify(x => x.StartNewAsync(It.IsAny<string>(), It.IsAny<string>()), 
                Times.Never);
        }

        [Test]
        public async Task Starts_Orchestrator_When_Does_Not_Find_Existing_Instances()
        {
            // Arrange
            _instances = new OrchestrationStatusQueryResult { DurableOrchestrationState = new List<DurableOrchestrationStatus>() };
            _starter.Setup(x => x.ListInstancesAsync(It.Is<OrchestrationStatusQueryCondition>(
                    c =>
                        c.InstanceIdPrefix == _orchestratorName
                        && c.RuntimeStatus.SequenceEqual(DurableOrchestrationClientExtensions.RuntimeStatuses)
                ), CancellationToken.None))
                .ReturnsAsync(_instances);


            // Act
            await _starter.Object.StartIfNotRunning(_orchestratorName);

            // Assert
            _starter.Verify(
                x => x.StartNewAsync(_orchestratorName, It.Is<string>(s => s.StartsWith(_orchestratorName))),
                Times.Once);
        }
    }
}
