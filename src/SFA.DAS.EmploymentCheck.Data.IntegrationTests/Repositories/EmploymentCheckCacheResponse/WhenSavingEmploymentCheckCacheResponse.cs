using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenSavingEmploymentCheckCacheResponse : RepositoryTestBase
    {
        private IEmploymentCheckCacheResponseRepository _sut;
        private Models.EmploymentCheckCacheResponse _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new EmploymentCheckCacheResponseRepository(Settings);

            var expected = Fixture.Create<Models.EmploymentCheckCacheResponse>();

            // Act
            await _sut.Insert(expected);

            // Assert
            _actual = await Get<Models.EmploymentCheckCacheResponse>(expected.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

