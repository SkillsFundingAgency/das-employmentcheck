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
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.HouseKeeping
{
    public class WhenCleanupWorkflowsTimer
    {
        [Test]
        public async Task ThenRunningCleanupWorkflowsTimer()
        {
            // Arrange
            var sut = new CleanupWorkflowsTimer();
            var purgeHistoryResult = new PurgeHistoryResult(0);
            var schedule = new DailySchedule("2:00:00");
            var timerInfo = new TimerInfo(schedule, It.IsAny<ScheduleStatus>());
            var mockDurableOrchestrationClient = new Mock<IDurableOrchestrationClient>();
            var mockLogger = new Mock<ILogger>();

            mockDurableOrchestrationClient.Setup(x => x.PurgeInstanceHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<IEnumerable<OrchestrationStatus>>())).Returns(Task.FromResult(purgeHistoryResult));

            // Act
            await sut.CleanupOldWorkflows(timerInfo, mockDurableOrchestrationClient.Object, mockLogger.Object);

            // Assert
            mockDurableOrchestrationClient.Verify(c => c.PurgeInstanceHistoryAsync(DateTime.MinValue,
                It.Is<DateTime>(d => IsCloseToNow(d)), It.Is<List<OrchestrationStatus>>(
                    x => x.SequenceEqual(ExpectedRuntimeStatuses))));

            mockLogger.Verify(m => m.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith("Scheduled cleanup done, 0 instances deleted")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        public OrchestrationStatus[] ExpectedRuntimeStatuses => new[]
        {
            OrchestrationStatus.Completed,
            OrchestrationStatus.Canceled,
            OrchestrationStatus.ContinuedAsNew,
            OrchestrationStatus.Failed,
            OrchestrationStatus.Terminated
        };

        private static bool IsCloseToNow(DateTime dateTime)
        {
            return (DateTime.Now - dateTime).TotalMinutes < 1;
        }
    }
}
