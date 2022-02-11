using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using DurableTask.Core.Tracing;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.TriggerHelperTests
{
    public class WhenGettingRunningInstances
    {
        private readonly string _orchestratorName;
        private readonly string _instanceIdPrefix;
        private readonly Mock<IDurableOrchestrationClient> _starter;
        private readonly Mock<ILogger> _log;
        private readonly OrchestrationStatusQueryResult _instances;
        private readonly Fixture _fixture;

        public WhenGettingRunningInstances()
        {
            _fixture = new Fixture();
            _orchestratorName = _fixture.Create<string>();
            _instanceIdPrefix = _fixture.Create<string>();
            _starter = new Mock<IDurableOrchestrationClient>();
            _instances = _fixture.Create<OrchestrationStatusQueryResult>();
            _log = new Mock<ILogger>();
        }

        [Test]
        public async Task And_There_Are_Instances_Running_Then_The_Starter_Is_Called_And_Returns_The_Instances()
        {
            //Arrange
            
            _starter.Setup(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), System.Threading.CancellationToken.None)).ReturnsAsync(_instances);

            var sut = new TriggerHelper();

            //Act

            var result = await sut.GetRunningInstances(_orchestratorName, _instanceIdPrefix, _starter.Object, _log.Object);

            //Assert
            _starter.Verify(
                x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), CancellationToken.None),
                Times.Once);
            Assert.AreEqual(_instances, result);
        }

        [Test]
        public async Task And_There_Are_No_Instances_Running_Then_Null_Is_Returned()
        {
            //Arrange

            _starter.Setup(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), System.Threading.CancellationToken.None)).ReturnsAsync((OrchestrationStatusQueryResult)null);

            var sut = new TriggerHelper();

            //Act

            var result = await sut.GetRunningInstances(_orchestratorName, _instanceIdPrefix, _starter.Object, _log.Object);

            //Assert
            Assert.AreEqual(null, result);
        }
    }
}