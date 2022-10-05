using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheckCacheRequest
{
    public class WhenGetEmploymentCheckCacheRequest : RepositoryTestBase
    {
        private IEmploymentCheckCacheRequestRepository _sut;

        [Test]
        public async Task Then_The_Checks_With_Lowest_Number_Of_PAYE_Schemes_Are_Performed_First()
        {
            // Arrange
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .ReturnsAsync(new HmrcApiRateLimiterOptions { EmploymentCheckBatchSize = 1 });

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var check1 = await CreateStartedEmploymentCheck();
            await CreatePendingHmrcApiRequest(check1, 10);

            var check2 = await CreateStartedEmploymentCheck();
            await CreatePendingHmrcApiRequest(check2, 20);

            var check3 = await CreateStartedEmploymentCheck();
            await CreatePendingHmrcApiRequest(check3, 30);

            // Act
            var actual = await _sut.GetEmploymentCheckCacheRequests();

            // Assert
            actual.First().ApprenticeEmploymentCheckId.Should().Be(check1.Id);
        }

        [Test]
        public async Task Then_The_Checks_With_Single_PAYE_Scheme_Are_Ordered_By_Created_DateTime()
        {
            // Arrange
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .ReturnsAsync(new HmrcApiRateLimiterOptions { EmploymentCheckBatchSize = 1 });

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var check1 = await CreateStartedEmploymentCheck();
            await CreatePendingHmrcApiRequest(check1, 1);

            var check2 = await CreateStartedEmploymentCheck();
            await CreatePendingHmrcApiRequest(check2, 1);

            var check3 = await CreateStartedEmploymentCheck();
            await CreatePendingHmrcApiRequest(check3, 1);

            var firstCreatedCheck = new[] { check1, check2, check3 }.OrderBy(c => c.CreatedOn).First();

            // Act
            var actual = await _sut.GetEmploymentCheckCacheRequests();

            // Assert
            actual.First().ApprenticeEmploymentCheckId.Should().Be(firstCreatedCheck.Id);
        }

        [Test]
        public async Task Then_The_GetHmrcRateLimiterOptions_Is_called()
        {
            // Arrange
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .ReturnsAsync(new HmrcApiRateLimiterOptions());

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var check1 = await CreateStartedEmploymentCheck();
            await CreatePendingHmrcApiRequest(check1, 1);

            // Act
            await _sut.GetEmploymentCheckCacheRequests();

            // Assert
            hmrcApiOptionsRepositoryMock.Verify(x => x.GetHmrcRateLimiterOptions(), Times.Once);
        }

        private async Task<Models.EmploymentCheck> CreateStartedEmploymentCheck()
        {
            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(c => c.RequestCompletionStatus, (short)ProcessingCompletionStatus.Started)
                .Create();

            await Insert(check);
            return check;
        }

        private async Task CreatePendingHmrcApiRequest(Models.EmploymentCheck check, int noOfRequests)
        {
            var request = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(r => r.ApprenticeEmploymentCheckId, check.Id)
                .With(r => r.CorrelationId, check.CorrelationId)
                .Without(r => r.RequestCompletionStatus)
                .CreateMany(noOfRequests);

            await Insert(request);
        }
    }
}
