using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.StoreEmploymentCheckResult.HandlerTests
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<IEmploymentCheckClient> _employmentCheckClient;
        private EmploymentCheckCacheRequest _employmentCheckCacheRequest;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckClient = new Mock<IEmploymentCheckClient>();
            _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
        }

        [Test]
        public async Task Then_The_EmploymentCheckClient_Is_Called()
        {
            // Arrange
            var command = new StoreEmploymentCheckResultCommand(_employmentCheckCacheRequest);

            _employmentCheckClient.Setup(x => x.StoreEmploymentCheckResult(command.EmploymentCheckCacheRequest))
                .ReturnsAsync(command.EmploymentCheckCacheRequest.Id);

            var sut = new StoreEmploymentCheckResultCommandHandler(_employmentCheckClient.Object);

            // Act
            await sut.Handle(new StoreEmploymentCheckResultCommand(_employmentCheckCacheRequest), CancellationToken.None);

            // Assert
            _employmentCheckClient.Verify(x => x.StoreEmploymentCheckResult(command.EmploymentCheckCacheRequest), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_The_Id_Of_The_Row()
        {
            // Arrange
            var command = new StoreEmploymentCheckResultCommand(_employmentCheckCacheRequest);

            _employmentCheckClient.Setup(x => x.StoreEmploymentCheckResult(command.EmploymentCheckCacheRequest))
                .ReturnsAsync(command.EmploymentCheckCacheRequest.Id);

            var sut = new StoreEmploymentCheckResultCommandHandler(_employmentCheckClient.Object);

            // Act
            var result = await sut.Handle(new StoreEmploymentCheckResultCommand(_employmentCheckCacheRequest), CancellationToken.None);

            // Assert
            result.EmploymentCheckId.Should().Equals(command.EmploymentCheckCacheRequest.Id);
        }
    }
}