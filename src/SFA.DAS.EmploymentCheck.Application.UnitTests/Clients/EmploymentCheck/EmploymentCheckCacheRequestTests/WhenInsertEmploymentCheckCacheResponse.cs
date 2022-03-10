using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Clients.EmploymentCheck.EmploymentCheckCacheRequestTests
{
    public class WhenInsertEmploymentCheckCacheResponse
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _sut = new EmploymentCheckService(
                Mock.Of<IEmploymentCheckRepository>(),
                Mock.Of<IEmploymentCheckCacheRequestRepository>(),
                _unitOfWorkMock.Object
            );
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            // Arrange
            var expected = _fixture.Create<EmploymentCheckCacheResponse>();

            // Act
            await _sut.InsertEmploymentCheckCacheResponse(expected);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.InsertAsync(expected), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        }
    }
}