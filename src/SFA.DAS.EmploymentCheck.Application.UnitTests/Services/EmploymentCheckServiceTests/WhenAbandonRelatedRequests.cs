using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenAbandonRelatedRequests
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepositoryMock;
        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCacheRequestRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();
            _employmentCheckCacheRequestRepositoryMock = new Mock<IEmploymentCheckCacheRequestRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _sut = new EmploymentCheckService(
                _employmentCheckRepositoryMock.Object,
                _employmentCheckCacheRequestRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Test]
        public async Task Then_SetRelatedRequestsRequestCompletionStatus_is_called_When_result_is_Employed()
        {
            // Arrange
            var request = _fixture.Build<EmploymentCheckCacheRequest>().With(x => x.Employed, true).Create();

            // Act
            await _sut.AbandonRelatedRequests(new[] { request });

            // Assert
            _employmentCheckCacheRequestRepositoryMock.Verify(x => 
                x.AbandonRelatedRequests(request, _unitOfWorkMock.Object), Times.Once());
        }

        [Test]
        public async Task Then_SetRelatedRequestsRequestCompletionStatus_is_not_called_When_result_is_not_Employed()
        {
            // Arrange
            var request = _fixture.Build<EmploymentCheckCacheRequest>().With(x => x.Employed, false).Create();

            // Act
            await _sut.AbandonRelatedRequests(new[] { request });

            // Assert
            _employmentCheckCacheRequestRepositoryMock.Verify(x => 
                x.AbandonRelatedRequests(request, _unitOfWorkMock.Object), Times.Never());
        }

    }
}
