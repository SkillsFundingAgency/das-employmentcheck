using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.CreateEmploymentCheckCacheRequestsActivityTests
{
    public class WhenCallingCreate
    {
        private Fixture _fixture;
        private Mock<IMediator> _mediator;
        private EmploymentCheckData _employmentCheckData;

        [SetUp]
        public void SetUp()

        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _employmentCheckData = _fixture.Create<EmploymentCheckData>();
        }

        [Test]
        public async Task Then_The_Command_Was_Executed()
        {
            // Arrange
            _mediator.Setup(x => x.Send(It.IsAny<CreateEmploymentCheckCacheRequestCommand>(), It.IsAny<CancellationToken>())).Verifiable();

            var sut = new CreateEmploymentCheckCacheRequestActivity(_mediator.Object);

            // Act
            await sut.Create(_employmentCheckData);

            // Assert
            _mediator.Verify(x => x.Send(It.IsAny<CreateEmploymentCheckCacheRequestCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}