using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.CreateEmploymentCheckCacheRequestsActivityTests
{
    public class WhenCallingCreate
    {
        private readonly Fixture _fixture;
        private readonly Mock<IMediator> _mediator;
        private readonly EmploymentCheckCacheRequest _employmentCheckCacheRequest;
        private readonly EmploymentCheckData _employmentCheckData;

        public WhenCallingCreate()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
            _employmentCheckData = _fixture.Create<EmploymentCheckData>();
        }

        [Test]
        public async Task Then_The_EmploymentCheckCacheRequest_Is_Returned()
        {
            //Arrange
            var sut = new CreateEmploymentCheckCacheRequestsActivity(_mediator.Object);

            var commandResult = new CreateEmploymentCheckCacheRequestCommandResult(_employmentCheckCacheRequest);

            _mediator.Setup(x => x.Send(It.IsAny<CreateEmploymentCheckCacheRequestCommand>(), CancellationToken.None))
                .ReturnsAsync(commandResult);

            //Act
            var result = await sut.Create(_employmentCheckData);

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(commandResult.EmploymentCheckCacheRequest, result);
        }
    }
}