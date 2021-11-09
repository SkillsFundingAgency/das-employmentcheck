using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Triggers.EmploymentCheckTimer
{
    public class WhenRunningTimerTrigger
    {
        private readonly Mock<IDurableOrchestrationClient> _starter;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<TimerInfo> _timer;

        public WhenRunningTimerTrigger()
        {
            _starter = new Mock<IDurableOrchestrationClient>();
            _logger = new Mock<ILogger>();
            _timer = new Mock<TimerInfo>(new DailySchedule("1"), new ScheduleStatus());
        }

        [Fact(Skip = "Not fully implemented yet")]
        public async void Then_The_Instance_Id_Is_Created()
        {
            //Arrange
            var sut = new Functions.AzureFunctions.Triggers.EmploymentCheckTimer();
            var instanceId = "test";

            _starter.Setup(x => x.StartNewAsync(nameof(TestEmploymentCheckOrchestrator), null))
                .ReturnsAsync(instanceId);

            //Act
            await sut.Run(default, _starter.Object, _logger.Object);

            //Assert
        }
    }
}