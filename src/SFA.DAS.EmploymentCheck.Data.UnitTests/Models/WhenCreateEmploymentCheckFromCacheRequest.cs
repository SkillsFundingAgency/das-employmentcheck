using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.Models
{
    public class WhenCreateEmploymentCheckFromCacheRequest
    {
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
        }

        [Test]
        public void Then_The_EmploymentCheck_Is_Created_With_ErrorType_Null()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();

            // Act
            var result = Data.Models.EmploymentCheck.CreateEmploymentCheck(request);

            // Assert
            result.Id.Should().Be(request.ApprenticeEmploymentCheckId);
            result.Employed.Should().Be(request.Employed);
            result.RequestCompletionStatus.Should().Be(request.RequestCompletionStatus);
            result.ErrorType.Should().Be(null);
        }

        [Test]
        public void Then_The_EmploymentCheck_Is_Created_Then_Is_Completed()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            var result = Data.Models.EmploymentCheck.CreateEmploymentCheck(request);

            // Act
            result.SetRequestCompletionStatus(Domain.Enums.ProcessingCompletionStatus.Completed);

            // Assert
            result.Id.Should().Be(request.ApprenticeEmploymentCheckId);
            result.Employed.Should().Be(request.Employed);
            result.RequestCompletionStatus.Should().Be((short)Domain.Enums.ProcessingCompletionStatus.Completed);
            result.ErrorType.Should().Be(null);
        }
       

        [Test]
        public void Then_The_EmploymentCheck_Is_Created_With_ErrorType_HmrcFailure()
        {
            // Arrange
            var request = _fixture.Build<EmploymentCheckCacheRequest>().Without(x => x.Employed).Create();

            // Act
            var result = Data.Models.EmploymentCheck.CreateEmploymentCheck(request);

            // Assert
            result.Id.Should().Be(request.ApprenticeEmploymentCheckId);
            result.Employed.Should().BeNull();
            result.RequestCompletionStatus.Should().Be(request.RequestCompletionStatus);
            result.ErrorType.Should().Be("HmrcFailure");
        }

    }
}
