using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheckCacheRequest
{
    public class WhenInsertingOrUpdatingEmploymentCheckCacheRequestUpdate
        : RepositoryTestBase
    {
        private IEmploymentCheckCacheRequestRepository _sut;
        private Models.EmploymentCheckCacheRequest _actual;

        [Test]
        public async Task CanUpdate()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestRepository(Settings);
            var saved = Fixture.Create<Models.EmploymentCheckCacheRequest>();

            await Insert(saved);

            var expected = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(e => e.Id, saved.Id)
                .Create();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = await Get<Models.EmploymentCheckCacheRequest>(saved.Id);

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                );

            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeCloseTo(expected.LastUpdatedOn.Value, TimeSpan.FromSeconds(1));
            _actual.Id.Should().BeGreaterThan(0);
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }
    }
}

