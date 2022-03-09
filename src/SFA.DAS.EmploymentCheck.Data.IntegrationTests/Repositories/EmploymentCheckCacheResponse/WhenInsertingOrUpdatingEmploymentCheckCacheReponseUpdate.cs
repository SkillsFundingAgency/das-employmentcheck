using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenInsertingOrUpdatingEmploymentCheckCacheResponseUpdate
        : RepositoryTestBase
    {
        private IEmploymentCheckCacheResponseRepository _sut;
        private Models.EmploymentCheckCacheResponse _actual;

        [Test]
        public async Task CanUpdate()
        {
            // Arrange
            _sut = new EmploymentCheckCacheResponseRepository(Settings);
            var saved = Fixture.Create<Models.EmploymentCheckCacheResponse>();

            await Insert(saved);

            var expected = Fixture.Build<Models.EmploymentCheckCacheResponse>()
                .With(e => e.Id, saved.Id)
                .Create();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = await Get<Models.EmploymentCheckCacheResponse>(saved.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

