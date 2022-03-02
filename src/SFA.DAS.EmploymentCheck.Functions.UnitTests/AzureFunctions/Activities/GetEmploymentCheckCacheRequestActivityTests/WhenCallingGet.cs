using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.ProcessEmploymentCheckCacheRequest;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetEmploymentCheckCacheRequestActivityTests
{
    public class WhenCallingGet
    {
        private Fixture _fixture;
        private EmploymentCheckCacheRequest _request;
        private Mock<IMediator> _mediator;
        private Mock<ILogger<GetEmploymentCheckCacheRequestActivity>> _logger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetEmploymentCheckCacheRequestActivity>>();
            _request = _fixture.Create<EmploymentCheckCacheRequest>();
        }

        [Test]
        public async Task Then_The_EmploymentCheckCacheRequest_Is_Returned()
        {
            // Arrange
            var sut = new GetEmploymentCheckCacheRequestActivity(_mediator.Object, _logger.Object);

            var queryResult = new ProcessEmploymentCheckCacheRequestQueryResult();
            queryResult.EmploymentCheckCacheRequest = _request;

            _mediator
                .Setup(x => x.Send(It.IsAny<ProcessEmploymentCheckCacheRequestQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = await sut.GetEmploymentCheckRequestActivityTask(null);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.EmploymentCheckCacheRequest, result);
        }
    }
}