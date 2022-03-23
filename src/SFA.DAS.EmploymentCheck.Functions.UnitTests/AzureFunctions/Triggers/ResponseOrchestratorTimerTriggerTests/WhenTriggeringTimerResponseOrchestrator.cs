using AutoFixture;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.CreateEmploymentCheckRequestsHttpTriggerTests
{
    public class WhenTriggeringTimerResponseOrchestrators
    {
        private Mock<IDurableOrchestrationClient> _starter;
        private Mock<ILogger<ResponseOrchestrator>> _logger;
        private Mock<ITriggerHelper> _triggerHelper;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _starter = new Mock<IDurableOrchestrationClient>();
            _logger = new Mock<ILogger<ResponseOrchestrator>>();
            _triggerHelper = new Mock<ITriggerHelper>();
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_The_Instance_Id_Is_Created_When_no_other_instances_are_running()
        {
            // Arrange
            string instanceIdPrefix = "Response-";
            var instanceId = $"{instanceIdPrefix}{Guid.NewGuid()}";
            var timerInfo = default(TimerInfo);
            var instances = new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new List<DurableOrchestrationStatus>(0)
            };

            _triggerHelper.Setup(x => x.GetRunningInstances(nameof(ResponseOrchestratorTimerTrigger), instanceIdPrefix, _starter.Object, _logger.Object))
                .ReturnsAsync(instances);

            _starter
                .Setup(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), CancellationToken.None))
                .ReturnsAsync(instances);

            _starter
                .Setup(x => x.StartNewAsync(nameof(ResponseOrchestrator), instanceId))
                .ReturnsAsync($"{instanceId}");

            //Act
            await ResponseOrchestratorTimerTrigger.ResponseOrchestratorTimerTriggerTask(timerInfo, _starter.Object, _logger.Object);

            // Assert
            _starter.Verify(x => x.StartNewAsync(It.Is<string>(x => x.Contains(nameof(ResponseOrchestrator))), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Then_The_Instance_Id_Is_Not_Created_When_other_instances_are_running()
        {
            // Arrange
            string instanceIdPrefix = "Response-";
            var instanceId = $"{instanceIdPrefix}{Guid.NewGuid()}";
            var timerInfo = default(TimerInfo);
            var instances = new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new[] { new DurableOrchestrationStatus() }
            };

            _triggerHelper.Setup(x => x.GetRunningInstances(nameof(ResponseOrchestratorTimerTrigger), instanceIdPrefix, _starter.Object, _logger.Object))
                .ReturnsAsync(instances);

            _starter
                .Setup(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), CancellationToken.None))
                .ReturnsAsync(instances);

            _starter
                .Setup(x => x.StartNewAsync(nameof(ResponseOrchestrator), It.IsAny<string>()))
                .ReturnsAsync($"{instanceId}");

            //Act
            await ResponseOrchestratorTimerTrigger.ResponseOrchestratorTimerTriggerTask(timerInfo, _starter.Object, _logger.Object);

            // Assert
            _starter.Verify(x => x.StartNewAsync(It.Is<string>(x => x.Contains(nameof(ResponseOrchestrator))), It.IsAny<string>()), Times.Never);
        }
    }
}