using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.ProcessEmploymentCheckCacheRequest;
using System.Threading;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetEmploymentCheckCacheRequestActivityTests
{
    public class WhenCallingGet
    {
        private readonly Fixture _fixture;
        private readonly EmploymentCheckCacheRequest _request;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetEmploymentCheckCacheRequestActivity>> _logger;

        public WhenCallingGet()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetEmploymentCheckCacheRequestActivity>>();
            _request = _fixture.Create<EmploymentCheckCacheRequest>();
        }

        [Test]
        public void Then_The_EmploymentCheckCacheRequest_Is_Returned()
        {
            //Arrange
            var sut = new GetEmploymentCheckCacheRequestActivity(_mediator.Object, _logger.Object);

            var queryResult = new ProcessEmploymentCheckCacheRequestQueryResult(_request);

            _mediator.Setup(x => x.Send(It.IsAny<ProcessEmploymentCheckCacheRequestQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            //Act
            var result = sut.GetEmploymentCheckRequestActivityTask(null).Result;

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.EmploymentCheckCacheRequest, result);
        }
    }
}