using System.Collections.Generic;
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
        public async Task Then_Unique_Learners_Are_Returned_Ordered_by_Smallest_Number_Of_PAYE_Schemes()
        {
            // Arrange
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .ReturnsAsync(new HmrcApiRateLimiterOptions { EmploymentCheckBatchSize = 3 });

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var check1 = await CreateStartedEmploymentCheck();
            var checkCacheRequests1 = await CreatePendingHmrcApiRequest(check1, 10);

            var check2 = await CreateStartedEmploymentCheck();
            var checkCacheRequests2 = await CreatePendingHmrcApiRequest(check2, 20);

            var check3 = await CreateStartedEmploymentCheck();
            var checkCacheRequests3 = await CreatePendingHmrcApiRequest(check3, 30);

            // Act
            var actual = await _sut.GetEmploymentCheckCacheRequests();

            // Assert
            actual.Length.Should().Be(3);

            actual[0].Id.Should().Be(checkCacheRequests1.OrderBy(c => c.Id).First().Id);
            actual[1].Id.Should().Be(checkCacheRequests2.OrderBy(c => c.Id).First().Id);
            actual[2].Id.Should().Be(checkCacheRequests3.OrderBy(c => c.Id).First().Id);
        }

        [Test]
        public async Task Then_The_Checks_With_Single_PAYE_Scheme_Are_Ordered_By_checkCacheRequestId()
        {
            // Arrange
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .ReturnsAsync(new HmrcApiRateLimiterOptions { EmploymentCheckBatchSize = 3 });

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var check1 = await CreateStartedEmploymentCheck();
            var checkCacheRequests1 = await CreatePendingHmrcApiRequest(check1, 1);

            var check2 = await CreateStartedEmploymentCheck();
            var checkCacheRequests2 = await CreatePendingHmrcApiRequest(check2, 1);

            var check3 = await CreateStartedEmploymentCheck();
            var checkCacheRequests3 = await CreatePendingHmrcApiRequest(check3, 1);

            // Act
            var actual = await _sut.GetEmploymentCheckCacheRequests();

            // Assert
            actual[0].Id.Should().Be(checkCacheRequests1.OrderBy(c => c.Id).First().Id);
            actual[1].Id.Should().Be(checkCacheRequests2.OrderBy(c => c.Id).First().Id);
            actual[2].Id.Should().Be(checkCacheRequests3.OrderBy(c => c.Id).First().Id);
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
                .Without(c => c.Id)
                .Create();

            var id = await Insert(check);
            
            check.Id = id;

            return check;
        }

        private async Task<IList<Models.EmploymentCheckCacheRequest>> CreatePendingHmrcApiRequest(Models.EmploymentCheck check, int noOfRequests)
        {
            var requests = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(r => r.ApprenticeEmploymentCheckId, check.Id)
                .With(r => r.CorrelationId, check.CorrelationId)
                .Without(r => r.RequestCompletionStatus)
                .Without(r => r.LastUpdatedOn)
                .Without(r => r.Id)
                .CreateMany(noOfRequests)
                .ToList();

            for (var index = 0; index < requests.Count; index++)
            {
                var request = requests[index];
                request.PayeSchemePriority = index + 1;
                var id = await Insert(request);
                request.Id = id;
            }

            return requests;
        }
    }
}
