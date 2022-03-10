using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.StoreCompletedEmploymentCheckActivityTests
{
    public class WhenCallingStore
    {
        private Fixture _fixture;
        private Mock<ICommandDispatcher> _dispatcher;
        private Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<ICommandDispatcher>();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();
        }

        [Test]
        public async Task Then_The_Command_Was_Executed()
        {
            // Arrange
            _dispatcher.Setup(x => x.Send(It.IsAny<StoreCompletedEmploymentCheckCommand>(), It.IsAny<CancellationToken>())).Verifiable();
            var sut = new StoreCompletedEmploymentCheckActivity(_dispatcher.Object);

            // Act
            await sut.Store(_employmentCheck);

            // Assert
            _dispatcher.Verify(x => x.Send(It.IsAny<StoreCompletedEmploymentCheckCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}