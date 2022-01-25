using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheck
{
    public class WhenSaveEmploymentCheck
        : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;
        private Models.EmploymentCheck _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings);
            var expected = Fixture.Create<Models.EmploymentCheck>();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = (await GetAll<Models.EmploymentCheck>())
                .Single(x => x.CorrelationId == expected.CorrelationId);


            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                );

            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeCloseTo(expected.LastUpdatedOn, TimeSpan.FromSeconds(1));
            _actual.MinDate.Should().BeCloseTo(expected.MinDate, TimeSpan.FromSeconds(1));
            _actual.MaxDate.Should().BeCloseTo(expected.MaxDate, TimeSpan.FromSeconds(1));
            _actual.Id.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task CanSUpdate()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings);
            var expected = Fixture.Create<Models.EmploymentCheck>();

            // Act
            await base.Insert(expected);

            _actual = (await GetAll<Models.EmploymentCheck>())
                .Single(x => x.CorrelationId == expected.CorrelationId);

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                );

            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeCloseTo(expected.LastUpdatedOn, TimeSpan.FromSeconds(1));
            _actual.MinDate.Should().BeCloseTo(expected.MinDate, TimeSpan.FromSeconds(1));
            _actual.MaxDate.Should().BeCloseTo(expected.MaxDate, TimeSpan.FromSeconds(1));
            _actual.Id.Should().BeGreaterThan(0);

            await Task.Delay(1000);
            expected.CreatedOn = DateTime.Now;
            expected.LastUpdatedOn = DateTime.Now;
            expected.MinDate = DateTime.Now;
            expected.MaxDate = DateTime.Now;

            await _sut.InsertOrUpdate(expected);

            // Assert
            _actual = (await GetAll<Models.EmploymentCheck>())
                .Single(x => x.CorrelationId == expected.CorrelationId);

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                );

            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeCloseTo(expected.LastUpdatedOn, TimeSpan.FromSeconds(1));
            _actual.MinDate.Should().BeCloseTo(expected.MinDate, TimeSpan.FromSeconds(1));
            _actual.MaxDate.Should().BeCloseTo(expected.MaxDate, TimeSpan.FromSeconds(1));
            _actual.Id.Should().BeGreaterThan(0);
        }

        [TearDown]
        public new async Task CleanUp()
        {
            await Delete(_actual);
            await base.CleanUp();
        }
    }
}

