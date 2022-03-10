using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.DataCollectionsResponse
{
    public class WhenSavingDataCollectionsResponse
        : RepositoryTestBase
    {
        private IDataCollectionsResponseRepository _sut;
        private Models.DataCollectionsResponse _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new DataCollectionsResponseRepository(Settings);
            var expected = Fixture.Create<Models.DataCollectionsResponse>();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = (await GetAll<Models.DataCollectionsResponse>())
                .Single(x => x.Id == expected.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

