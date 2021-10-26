using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
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
            _timer = new Mock<TimerInfo>();
        }

        [Fact (Skip = "Logger test helper not yet implemented")]
        public async void Then_The_Instance_Id_Is_Created()
        {
            //Arrange
            var sut = new Functions.AzureFunctions.Triggers.EmploymentCheckTimer();
            var instanceId = "test";

            _starter.Setup(x => x.StartNewAsync(nameof(TestEmploymentCheckOrchestrator), null))
                .ReturnsAsync(instanceId);

            //Act
            await sut.Run(_timer.Object, _starter.Object, _logger.Object);

            //Assert
            //_logger.Verify(x => x.LogInformation($"Auto Started EmploymentCheckOrchestrator with ID = '{instanceId}'."));
        }
    }
}