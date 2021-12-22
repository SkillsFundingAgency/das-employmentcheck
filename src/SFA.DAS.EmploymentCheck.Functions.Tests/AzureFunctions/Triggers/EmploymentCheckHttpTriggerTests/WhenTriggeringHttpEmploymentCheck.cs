using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Triggers.EmploymentCheckHttpTriggerTests
{
    public class WhenTriggeringHttpEmploymentCheck
    {
        private readonly Mock<HttpRequestMessage> _request;
        private readonly Mock<IDurableOrchestrationClient> _starter;
        private readonly Mock<ILogger> _logger;

        public WhenTriggeringHttpEmploymentCheck()
        {
            _request = new Mock<HttpRequestMessage>();
            _starter = new Mock<IDurableOrchestrationClient>();
            _logger = new Mock<ILogger>();
        }

        [Fact]
        public async Task Then_The_Instance_Id_Is_Created_When_no_other_instances_are_running()
        {
            //Arrange
            const string instanceId = "test";
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);

            _starter.Setup(x => x.StartNewAsync(nameof(ApprenticeEmploymentChecksOrchestrator), It.IsAny<string>()))
                .ReturnsAsync(instanceId);
            _starter.Setup(x => x.CreateCheckStatusResponse(_request.Object, instanceId, false))
                .Returns(response);

            var instances = new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new List<DurableOrchestrationStatus>(0)
            };

            _starter.Setup(x => x.ListInstancesAsync(
                    It.Is<OrchestrationStatusQueryCondition>(c => c.InstanceIdPrefix == "EmploymentCheck-")
                    , System.Threading.CancellationToken.None))
                .ReturnsAsync(instances);

            //Act
            var result = await EmploymentCheckHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            //Assert

            Assert.Equal(response, result);
            Assert.Equal(response.StatusCode, result.StatusCode);
        }

        [Fact]
        public async Task Then_The_Instance_Id_Is_Not_Created_When_other_instances_are_running()
        {
            //Arrange
            const string instanceId = "test";

            _starter.Setup(x => x.StartNewAsync(nameof(ApprenticeEmploymentChecksOrchestrator), It.IsAny<string>()))
                .ReturnsAsync(instanceId);

            var instances = new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new[] {new DurableOrchestrationStatus()}
            };

            _starter.Setup(x => x.ListInstancesAsync(
                    It.Is<OrchestrationStatusQueryCondition>(c => c.InstanceIdPrefix == "EmploymentCheck-")
                    , System.Threading.CancellationToken.None))
                .ReturnsAsync(instances);

            //Act
            var result = await EmploymentCheckHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        }
    }
}