using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Repositories;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenSavingEmploymentCheckCacheResponse : RepositoryTestBase
    {
        private IEmploymentCheckCacheResponseRepository _sut;
        private Functions.Application.Models.EmploymentCheckCacheResponse _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new EmploymentCheckCacheResponseRepository(Settings);

            var expected = Fixture.Create<Functions.Application.Models.EmploymentCheckCacheResponse>();

            // Act
            await _sut.Insert(expected);

            // Assert
            _actual = await Get<Functions.Application.Models.EmploymentCheckCacheResponse>(expected.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

