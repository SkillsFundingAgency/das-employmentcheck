using AutoFixture;
using DurableTask.Core;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.PurgeInstanceHistoryTests
{
    public class WhenTriggeringPurgeInstanceHistoryHttpTrigger
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
        public async Task Then_all_orchestrator_instance_history_is_cleared()
        {
            // Arrange
            var allStatuses = Enum.GetValues(typeof(OrchestrationStatus)).Cast<OrchestrationStatus>();
            var expected = _fixture.Create<PurgeHistoryResult>();
            _starter.Setup(s => s.PurgeInstanceHistoryAsync(DateTime.MinValue, null, allStatuses))
                .ReturnsAsync(expected);

            // Act
            var result = await PurgeInstanceHistoryHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.ReadAsStringAsync().Result.Should().Be($"PurgeInstanceHistoryHttpTrigger: cleanup done, {expected.InstancesDeleted} instances deleted.");
            _starter.VerifyAll();
        }
    }
}