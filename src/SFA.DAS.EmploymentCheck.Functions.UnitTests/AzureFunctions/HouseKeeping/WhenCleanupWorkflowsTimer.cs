using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.HouseKeeping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.HouseKeeping
{
    public class WhenCleanupWorkflowsTimer
    {
        [Test]
        public async Task ThenRunningCleanupWorkflowsTimer()
        {
            // Arrange
            PurgeHistoryResult purgeHistoryResult = new PurgeHistoryResult(0);
            TimerSchedule schedule = new DailySchedule("2:00:00");
            TimerInfo timerInfo = new TimerInfo(schedule, It.IsAny<ScheduleStatus>(), false);
            Mock<IDurableOrchestrationClient> mockDurableOrchestrationClient = new Mock<IDurableOrchestrationClient>();
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            CleanupWorkflowsTimer sut = new CleanupWorkflowsTimer();

            mockDurableOrchestrationClient.Setup(x => x.PurgeInstanceHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<IEnumerable<OrchestrationStatus>>())).Returns(Task.FromResult(purgeHistoryResult));
       

            // Act
            await sut.CleanupOldWorkflows(timerInfo, mockDurableOrchestrationClient.Object, mockLogger.Object);

            // Assert
            mockLogger.Verify(m => m.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().StartsWith("Scheduled cleanup done")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
