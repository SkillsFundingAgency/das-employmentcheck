using System;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheckCacheRequest
{
    public class WhenGetLearnerPayeCheckPriority : RepositoryTestBase
    {
        private IEmploymentCheckCacheRequestRepository _sut;
        private Fixture _fixture;

        [Test]
        public async Task Then_The_Checks_With_Employed_Equal_False_are_Ignored()
        {
            // Arrange
            _fixture = new Fixture();
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .Returns(new HmrcApiRateLimiterOptions { EmploymentCheckBatchSize = 1 });

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            await CreateEmploymentCheckCacheRequest("AB123456A", _fixture.Create<string>(), false);
            var check2 = await CreateEmploymentCheckCacheRequest("AB123456A", _fixture.Create<string>(), true);

            // Act
            var actual = await _sut.GetLearnerPayeCheckPriority("AB123456A");

            // Assert
            actual.Count.Should().Be(1);
            actual.Single().PayeScheme.Should().Be(check2.PayeScheme);
            actual.Single().PriorityOrder.Should().Be(1);
        }

        [Test]
        public async Task Then_The_Checks_With_Employed_Equal_True_are_Considered()
        {
            // Arrange
            _fixture = new Fixture();
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .Returns(new HmrcApiRateLimiterOptions { EmploymentCheckBatchSize = 1 });

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var check2 = await CreateEmploymentCheckCacheRequest("AB123456A", _fixture.Create<string>(), true);
            var check1 = await CreateEmploymentCheckCacheRequest("AB123456A", _fixture.Create<string>(), true);

            // Act
            var actual = await _sut.GetLearnerPayeCheckPriority("AB123456A");

            // Assert
            actual.Count.Should().Be(2);
            actual[0].PayeScheme.Should().Be(check1.PayeScheme);
            actual[0].PriorityOrder.Should().Be(1);
            actual[1].PayeScheme.Should().Be(check2.PayeScheme);
            actual[1].PriorityOrder.Should().Be(2);
        }

        [Test]
        public async Task Then_Returns_Empty_List_If_No_Data_For_Given_Nino()
        {
            // Arrange
            _fixture = new Fixture();
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .Returns(new HmrcApiRateLimiterOptions { EmploymentCheckBatchSize = 1 });

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            await CreateEmploymentCheckCacheRequest("AB123456A", _fixture.Create<string>(), true);
            await CreateEmploymentCheckCacheRequest("AB123456A", _fixture.Create<string>(), true);

            // Act
            var actual = await _sut.GetLearnerPayeCheckPriority("invalidNino");

            // Assert
            actual.Count.Should().Be(0);
        }

        [Test]
        public async Task Then_Returns_LearnerPayeCheckPriority_Ordered_by_CreatedOn()
        {
            // Arrange
            _fixture = new Fixture();
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .Returns(new HmrcApiRateLimiterOptions { EmploymentCheckBatchSize = 1 });

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var check2 = await CreateEmploymentCheckCacheRequest("AB123456A", _fixture.Create<string>(), true);
            var check1 = await CreateEmploymentCheckCacheRequest("AB123456A", _fixture.Create<string>(), true);

            // Act
            var actual = await _sut.GetLearnerPayeCheckPriority("AB123456A");

            // Assert
            check1.CreatedOn.Should().BeAfter(check2.CreatedOn);

            actual.Count.Should().Be(2);
            actual[0].PayeScheme.Should().Be(check1.PayeScheme);
            actual[0].PriorityOrder.Should().Be(1);
            actual[1].PayeScheme.Should().Be(check2.PayeScheme);
            actual[1].PriorityOrder.Should().Be(2);
        }

        [Test]
        public async Task Then_Returns_Unique_LearnerPayeCheckPriority_When_Duplicate_Exists()
        {
            // Arrange
            _fixture = new Fixture();
            var hmrcApiOptionsRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            hmrcApiOptionsRepositoryMock.Setup(x => x.GetHmrcRateLimiterOptions())
                .Returns(new HmrcApiRateLimiterOptions { EmploymentCheckBatchSize = 1 });

            _sut = new EmploymentCheckCacheRequestRepository(Settings, hmrcApiOptionsRepositoryMock.Object, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var paye = _fixture.Create<string>();

            var check2 = await CreateEmploymentCheckCacheRequest("AB123456A", paye, true);
            var check1 = await CreateEmploymentCheckCacheRequest("AB123456A", paye, true);

            // Act
            var actual = await _sut.GetLearnerPayeCheckPriority("AB123456A");

            // Assert
            check1.CreatedOn.Should().BeAfter(check2.CreatedOn);

            actual.Count.Should().Be(1);
            actual[0].PayeScheme.Should().Be(check1.PayeScheme);
            actual[0].PriorityOrder.Should().Be(1);
        }

        private async Task<Models.EmploymentCheckCacheRequest> CreateEmploymentCheckCacheRequest(string nino, string payeScheme, bool employed)
        {
            var request = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(r => r.Nino, nino)
                .With(r => r.Employed, employed)
                .With(r => r.PayeScheme, payeScheme)
                .With(r => r.CreatedOn, DateTime.UtcNow)
                .Without(r => r.Id)
                .Create();


            var id = await Insert(request);
            request.Id = id;

            return request;
        }
    }
}
