using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System;
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
            _sut = new EmploymentCheckCacheRequestRepository(Settings, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var lowestPayeSchemeCorrelationId = Guid.NewGuid();
            var requests1 = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(r => r.CorrelationId, lowestPayeSchemeCorrelationId)
                .Without(r => r.RequestCompletionStatus)
                .CreateMany(10);

            var requests2 = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(r => r.CorrelationId, Guid.NewGuid())
                .Without(r => r.RequestCompletionStatus)
                .CreateMany(20);
            
            var requests3 = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(r => r.CorrelationId, Guid.NewGuid())
                .Without(r => r.RequestCompletionStatus)
                .CreateMany(30);

            await Insert(requests3);
            await Insert(requests2);
            await Insert(requests1);

            // Act
            var actual = await _sut.GetEmploymentCheckCacheRequest();

            // Assert
            actual.CorrelationId.Should().Be(lowestPayeSchemeCorrelationId);
        }
    }
}
