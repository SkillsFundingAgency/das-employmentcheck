using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Commands;
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
        private Mock<ICommandDispatcher> _dispatcher;
        private EmploymentCheckData _employmentCheckData;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<ICommandDispatcher>();

            _employmentCheckData = _fixture
                .Create<EmploymentCheckData>();
        }

        [Test]
        public async Task Then_The_Command_Was_Executed()
        {
            // Arrange
            var sut = new CreateEmploymentCheckCacheRequestActivity(_dispatcher.Object);

            // Act
            await sut.Create(_employmentCheckData);

            // Assert
            _dispatcher.Verify(d => d.Send(
                It.Is<CreateEmploymentCheckCacheRequestCommand>(
                    c => c.EmploymentCheckData == _employmentCheckData
                ), CancellationToken.None
            ), Times.Once);
        }
    }
}