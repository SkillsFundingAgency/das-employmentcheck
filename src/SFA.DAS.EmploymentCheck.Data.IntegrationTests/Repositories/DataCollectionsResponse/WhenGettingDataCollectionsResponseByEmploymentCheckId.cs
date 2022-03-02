using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using System;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

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
            _actual.Should().BeEquivalentTo(saved,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                );

            _actual.CreatedOn.Should().BeCloseTo(saved.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeCloseTo(saved.LastUpdatedOn.Value, TimeSpan.FromSeconds(1));
            _actual.Id.Should().BeGreaterThan(0);
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }
    }
}

