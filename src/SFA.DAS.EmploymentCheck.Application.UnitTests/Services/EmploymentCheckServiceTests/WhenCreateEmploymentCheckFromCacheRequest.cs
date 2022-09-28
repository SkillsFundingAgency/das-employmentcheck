using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenCreateEmploymentCheckFromCacheRequest
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
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
