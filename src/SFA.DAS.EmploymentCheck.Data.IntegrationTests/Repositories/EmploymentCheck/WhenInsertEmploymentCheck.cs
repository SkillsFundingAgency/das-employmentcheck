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
    public class WhenInsertEmploymentCheck
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
            _actual = (await Get<Models.EmploymentCheck>(expected.Id));

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                );

            _actual.Id.Should().Be(expected.Id);
            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeCloseTo(expected.LastUpdatedOn.Value, TimeSpan.FromSeconds(1));
            _actual.MinDate.Should().BeCloseTo(expected.MinDate, TimeSpan.FromSeconds(1));
            _actual.MaxDate.Should().BeCloseTo(expected.MaxDate, TimeSpan.FromSeconds(1));
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }
    }
}

