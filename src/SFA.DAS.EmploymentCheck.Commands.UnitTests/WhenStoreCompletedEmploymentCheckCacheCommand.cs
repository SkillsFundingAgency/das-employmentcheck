using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests
{
    public class WhenStoreCompletedEmploymentCheckCacheCommand
    {
        private StoreCompletedEmploymentCheckCommandHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>();
            _sut = new StoreCompletedEmploymentCheckCommandHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var request = _fixture.Create<StoreCompletedEmploymentCheckCommand>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.StoreCompletedEmploymentCheck(request.EmploymentCheck), Times.Once);
        }

        [Test]
        public void Then_EmploymentCheck_Is_Retrieved()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Models.EmploymentCheck>();

            // Act
            var cmd = new StoreCompletedEmploymentCheckCommand(employmentCheck);

            // Assert
            Assert.AreEqual(employmentCheck, cmd.EmploymentCheck);
        }
    }
}
