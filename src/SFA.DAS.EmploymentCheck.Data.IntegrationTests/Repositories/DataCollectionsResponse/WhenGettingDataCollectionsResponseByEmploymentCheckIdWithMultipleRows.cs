using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.DataCollectionsResponse
{
    public class WhenGettingDataCollectionsResponseByEmploymentCheckIdWithMultipleRows
        : RepositoryTestBase
    {
        private IDataCollectionsResponseRepository _sut;
        private Models.DataCollectionsResponse _actual;

        [Test]
        public async Task Get_Should_Return_The_Latest_Single_Row_When_There_Are_Multiple_Rows_With_The_Same_ForeignKey()
        {
            // Arrange
            _sut = new DataCollectionsResponseRepository(Settings);
            var savedFirst = Fixture
                .Build<Models.DataCollectionsResponse>()
                .With(e => e.CreatedOn, DateTime.Now)
                .Create();

            await Insert(savedFirst);

            var savedLast = Fixture
                .Build<Models.DataCollectionsResponse>()
                .With(e => e.ApprenticeEmploymentCheckId, savedFirst.ApprenticeEmploymentCheckId)
                .With(e => e.CreatedOn, DateTime.Now.AddHours(1))
                .Create();

            await Insert(savedLast);

            // Act
            _actual = await _sut.GetByEmploymentCheckId(savedLast.ApprenticeEmploymentCheckId.Value);

            // Assert
            _actual.Should().BeEquivalentTo(savedLast,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                );

            _actual.CreatedOn.Should().BeCloseTo(savedLast.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeCloseTo(savedLast.LastUpdatedOn.Value, TimeSpan.FromSeconds(1));
            _actual.Id.Should().BeGreaterThan(savedFirst.Id);
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }
    }
}

