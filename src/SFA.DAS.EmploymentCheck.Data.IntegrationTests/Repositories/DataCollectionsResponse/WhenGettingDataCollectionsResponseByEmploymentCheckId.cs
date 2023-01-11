using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.DataCollectionsResponse
{
    public class WhenGettingDataCollectionsResponseByEmploymentCheckId
        : RepositoryTestBase
    {
        private IDataCollectionsResponseRepository _sut;
        private Models.DataCollectionsResponse _actual;

        [Test]
        public async Task CanGet()
        {
            // Arrange
            _sut = new DataCollectionsResponseRepository(Settings);
            var saved = Fixture.Create<Models.DataCollectionsResponse>();

            await Insert(saved);

            // Act
            _actual = await _sut.GetByEmploymentCheckId(saved.ApprenticeEmploymentCheckId.Value);

            // Assert
            _actual.Should().BeEquivalentTo(saved);
        }

        [Test]
        public void CanCreate()
        {
            // Arrange 
            var expected = Fixture.Create<Models.DataCollectionsResponse>();

            // Act
            var actual = new Models.DataCollectionsResponse(
                expected.Id, 
                expected.ApprenticeEmploymentCheckId, 
                expected.CorrelationId,
                expected.Uln,
                expected.NiNumber,
                expected.HttpResponse,
                expected.HttpStatusCode);

            actual.CreatedOn = expected.CreatedOn;
            actual.LastUpdatedOn = expected.LastUpdatedOn; 

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

