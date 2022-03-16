using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.DataCollectionsResponse
{
    public class WhenInsertingOrUpdatingDataCollectionsResponseUpdate
        : RepositoryTestBase
    {
        private IDataCollectionsResponseRepository _sut;
        private Models.DataCollectionsResponse _actual;

        [Test]
        public async Task CanUpdate()
        {
            // Arrange
            _sut = new DataCollectionsResponseRepository(Settings);
            var saved = Fixture.Create<Models.DataCollectionsResponse>();

            await Insert(saved);

            var expected = Fixture.Build<Models.DataCollectionsResponse>()
                .With(e => e.Id, saved.Id)
                .Create();

            // Act
            await _sut.InsertOrUpdate(expected);

            // Assert
            _actual = await Get<Models.DataCollectionsResponse>(saved.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

