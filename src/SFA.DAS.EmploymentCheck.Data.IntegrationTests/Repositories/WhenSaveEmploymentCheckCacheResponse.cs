using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenSaveEmploymentCheckCacheResponse : RepositoryTestBase
    {
        private IEmploymentCheckCacheResponseRepository _sut;
        private EmploymentCheckCacheResponse _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new EmploymentCheckCacheResponseRepository(Settings);

            var expected = Fixture.Create<EmploymentCheckCacheResponse>();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = await Get<EmploymentCheckCacheResponse>(expected.Id);

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.CreatedOn)
                );
            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
        }

        [TearDown]
        public new async Task CleanUp()
        {
            await Delete(_actual);
            await base.CleanUp();
        }

    }
}

