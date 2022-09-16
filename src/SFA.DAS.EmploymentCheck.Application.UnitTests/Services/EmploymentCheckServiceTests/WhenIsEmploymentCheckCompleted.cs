using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenIsEmploymentCheckCompleted
    {
        private IEmploymentCheckService _sut;
        private Mock<IEmploymentCheckRepository> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IEmploymentCheckRepository>();

            _sut = new EmploymentCheckService(
                _repositoryMock.Object,
                Mock.Of<IEmploymentCheckCacheRequestRepository>(),
                Mock.Of<IUnitOfWork>()
            );
        }

        [Test]
        public async Task Then_The_Repository_Is_Called()
        {
            // Act
            await _sut.IsEmploymentCheckCompleted(It.IsAny<long>());

            // Assert
            _repositoryMock.Verify(x => x.IsEmploymentCheckCompleted(It.IsAny<long>()), Times.AtLeastOnce());
        }

        [Test]
        public async Task And_The_Repository_Returns_True_Then_True_Returned()
        {
            // Arrange
            _repositoryMock.Setup(x => x.IsEmploymentCheckCompleted(It.IsAny<long>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.IsEmploymentCheckCompleted(It.IsAny<long>());

            // Assert
            result.Should().BeTrue();
        }
    }
}