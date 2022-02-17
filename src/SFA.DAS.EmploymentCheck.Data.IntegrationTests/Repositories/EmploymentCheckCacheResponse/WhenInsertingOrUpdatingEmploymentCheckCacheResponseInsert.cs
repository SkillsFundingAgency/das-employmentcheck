using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenInsertingOrUpdatingEmploymentCheckCacheResponseInsert
        : RepositoryTestBase
    {
        private IEmploymentCheckCacheResponseRepository _sut;
        private Models.EmploymentCheckCacheResponse _actual;

        [Test]
        public async Task CanInsert()
        {
            // Arrange
            _sut = new EmploymentCheckCacheResponseRepository(Settings);
            var expected = Fixture.Create<Models.EmploymentCheckCacheResponse>();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = (await GetAll<Models.EmploymentCheckCacheResponse>())
                .Single(x => x.Id == expected.Id);

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                );

            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeNull();
            _actual.Id.Should().BeGreaterThan(0);
        }
    }
}

