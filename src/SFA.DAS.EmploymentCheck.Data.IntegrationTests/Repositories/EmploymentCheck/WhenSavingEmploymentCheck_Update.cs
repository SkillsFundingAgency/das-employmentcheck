using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheck
{
    public class WhenSavingEmploymentCheck_Update
        : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;
        private Models.EmploymentCheck _actual;

        [Test]
        public async Task CanUpdate()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings);
            var saved = Fixture.Create<Models.EmploymentCheck>();
            await Insert(saved);

            var expected = Fixture.Build<Models.EmploymentCheck>()
                .With(e => e.Id, saved.Id)
                .Create();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = await Get<Models.EmploymentCheck>(saved.Id);

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                    // TODO: fix the _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromMilliseconds(1000)) below and remove the exclusions.
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                );

            _actual.Id.Should().Be(expected.Id);
            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromMilliseconds(1000));
            _actual.LastUpdatedOn.Should().BeCloseTo(expected.LastUpdatedOn.Value, TimeSpan.FromMilliseconds(1000));
            _actual.MinDate.Should().BeCloseTo(expected.MinDate, TimeSpan.FromSeconds(10));
            _actual.MaxDate.Should().BeCloseTo(expected.MaxDate, TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }
    }
}

