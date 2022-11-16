using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.EmploymentCheckTimerTriggerTests
{
    public class WhenEmploymentChecksTimerTrigger
    {
        [Test]
        public async Task ThenRunningEmploymentChecksTimerTrigger()
        {
            // Arrange
            var schedule = new DailySchedule("2:00:00");
            var timerInfo = new TimerInfo(schedule, It.IsAny<ScheduleStatus>());
            var mockDurableOrchestrationClient = new Mock<IDurableOrchestrationClient>();
            var mockLogger = new Mock<ILogger>();

            mockDurableOrchestrationClient.Setup(c => c.ListInstancesAsync(
                It.IsAny<OrchestrationStatusQueryCondition>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new List<DurableOrchestrationStatus>()
            });

            // Act
            await EmploymentChecksTimerTrigger.EmploymentChecksTimerTriggerTask(timerInfo, mockDurableOrchestrationClient.Object, mockLogger.Object);

            // Assert
            mockDurableOrchestrationClient.Verify(c =>
                    c.StartNewAsync("CreateEmploymentCheckCacheRequestsOrchestrator",
                        It.Is<string>(s => s.StartsWith("CreateEmploymentCheckCacheRequestsOrchestrator")))
                , Times.Once);

            mockDurableOrchestrationClient.Verify(c =>
                    c.StartNewAsync("ProcessEmploymentCheckRequestsOrchestrator",
                        It.Is<string>(s => s.StartsWith("ProcessEmploymentCheckRequestsOrchestrator")))
                , Times.Once);
        }
    }
}
