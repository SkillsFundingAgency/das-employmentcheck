using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheck.EmploymentCheckTests
{
    public class WhenStoreCompletedCheckWithEmploymentCheck
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
        public async Task Then_The_EmploymentCheck_is_Stored()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>()
                .With(x => x.Id, 1)
                .With(x => x.Employed, true)
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();

            // Act
            await _sut.StoreCompletedEmploymentCheck(employmentCheck);

            // Assert
            _employmentCheckRepositoryMock.Verify(r => r.InsertOrUpdate(It.Is<Data.Models.EmploymentCheck>(ec => ec.Id == employmentCheck.Id
                && ec.AccountId == employmentCheck.AccountId
                && ec.ApprenticeshipId == employmentCheck.ApprenticeshipId
                && ec.CheckType == employmentCheck.CheckType
                && ec.CorrelationId == employmentCheck.CorrelationId
                && ec.Employed == employmentCheck.Employed
                && ec.MinDate == employmentCheck.MinDate
                && ec.MaxDate == employmentCheck.MaxDate
                && ec.RequestCompletionStatus == employmentCheck.RequestCompletionStatus
                && ec.Uln == employmentCheck.Uln
                && ec.CreatedOn == employmentCheck.CreatedOn
                && ec.LastUpdatedOn == employmentCheck.LastUpdatedOn
                ))
                , Times.Once);
        }
    }
}
