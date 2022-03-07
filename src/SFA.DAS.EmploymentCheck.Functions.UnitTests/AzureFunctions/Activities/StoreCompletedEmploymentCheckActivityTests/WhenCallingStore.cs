using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;
using Models = SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using System.Threading;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;
using System;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.StoreCompletedEmploymentCheckActivityTests
{
    public class WhenCallingStore
    {
        private Fixture _fixture;
        private Mock<IMediator> _mediator;
        private Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()

        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();
        }

        [Test]
        public async Task Then_The_Command_Was_Executed()
        {
            // Arrange
            _mediator.Setup(x => x.Send(It.IsAny<StoreCompletedEmploymentCheckCommand>(), It.IsAny<CancellationToken>())).Verifiable();
            var sut = new StoreCompletedEmploymentCheckActivity(_mediator.Object);

            // Act
            await sut.Store(_employmentCheck);

            // Assert
            _mediator.Verify(x => x.Send(It.IsAny<StoreCompletedEmploymentCheckCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}