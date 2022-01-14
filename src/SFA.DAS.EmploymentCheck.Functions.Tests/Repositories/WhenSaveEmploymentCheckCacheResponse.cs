using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Repositories
{
    public class WhenSaveEmploymentCheckCacheResponse : RepositoryTestBase
    {
        private EmploymentCheckCacheResponseRepository _sut;

        [Test, Ignore("todo: switch to in-memory database")]
        public async Task CanSave()
        {
            // Arrange
            _sut = new EmploymentCheckCacheResponseRepository(Settings);

            var expected = Fixture.Create<EmploymentCheckCacheResponse>();

            // Act
            await _sut.Save(expected);

            // Assert
            var actual = (await GetAll<EmploymentCheckCacheResponse>())
                .Single(x => x.CorrelationId == expected.CorrelationId);


            actual.Should().BeEquivalentTo(expected, 
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                );
            actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            actual.Id.Should().BeGreaterThan(0);
        }
    }
}

