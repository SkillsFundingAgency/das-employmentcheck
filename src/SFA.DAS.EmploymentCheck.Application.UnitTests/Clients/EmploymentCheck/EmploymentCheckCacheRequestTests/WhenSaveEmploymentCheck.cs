using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Clients.EmploymentCheck.EmploymentCheckCacheRequestTests
{
    public class WhenSaveEmploymentCheck
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IEmploymentCheckRepository>();

            _sut = new EmploymentCheckService(
                Mock.Of<ILogger<IEmploymentCheckService>>(),
                _repositoryMock.Object,
                Mock.Of<IEmploymentCheckCacheRequestRepository>(),
                Mock.Of<IUnitOfWork>()
            );
        }

        [Test]
        public async Task The_Repository_InsertOrUpdate_Is_Called()
        {
            // Arrange
            var expected = _fixture.Create<Data.Models.EmploymentCheck>();

            // Act
            await _sut.SaveEmploymentCheck(expected);

            // Assert
            _repositoryMock.Verify(x => x.InsertOrUpdate(expected), Times.Once);
        }
    }
}