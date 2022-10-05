using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Commands.AbandonRelatedRequests;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.AbandonRelatedRequestsActivity
{
    public class WhenCallingCreate
    {
        private Fixture _fixture;
        private Mock<ICommandDispatcher> _dispatcher;
        private EmploymentCheckCacheRequest[] _employmentCheckCacheRequests;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<ICommandDispatcher>();

            _employmentCheckCacheRequests = _fixture.Create<EmploymentCheckCacheRequest[]>();
        }

        [Test]
        public async Task Then_The_Command_Was_Executed()
        {
            // Arrange
            var sut = new Functions.AzureFunctions.Activities.AbandonRelatedRequestsActivity(_dispatcher.Object);

            // Act
            await sut.Create(_employmentCheckCacheRequests);

            // Assert
            _dispatcher.Verify(d => d.Send(
                It.Is<AbandonRelatedRequestsCommand>(
                    c => c.EmploymentCheckCacheRequests == _employmentCheckCacheRequests
                ), CancellationToken.None
            ), Times.Once);
        }
    }
}