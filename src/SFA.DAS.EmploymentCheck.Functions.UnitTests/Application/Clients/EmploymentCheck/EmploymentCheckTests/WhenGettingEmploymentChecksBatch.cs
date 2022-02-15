using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckTests
{
    public class WhenGettingEmploymentChecksBatch
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();

            _sut = new EmploymentCheckService(
                Mock.Of<ILogger<IEmploymentCheckService>>(),
                _employmentCheckRepositoryMock.Object,
                Mock.Of<IEmploymentCheckCacheRequestRepository>(),
                Mock.Of<IUnitOfWork>()
            );
        }

        [Test]
        public async Task Then_The_Repository_Is_Called()
        {
            // Act
            await _sut.GetEmploymentChecksBatch();

            // Assert
            _employmentCheckRepositoryMock.Verify(x => x.GetEmploymentChecksBatch(), Times.AtLeastOnce());
        }

        [Test]
        public async Task And_The_Repository_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            // Arrange
            var employmentChecks = _fixture.CreateMany<Models.EmploymentCheck>().ToList();

            _employmentCheckRepositoryMock.Setup(x => x.GetEmploymentChecksBatch())
                .ReturnsAsync(employmentChecks);

            // Act
            var result = await _sut.GetEmploymentChecksBatch();

            // Assert
            Assert.AreEqual(employmentChecks, result);
        }
    }
}