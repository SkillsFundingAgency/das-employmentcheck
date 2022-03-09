using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Repositories;

namespace SFA.DAS.EmploymentCheck.Api.IntegrationTests.Repositories
{
    public class WhenInsertingEmploymentCheck : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;

        [Test]
        public async Task Then_The_Check_Should_Be_Saved()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings);

            var expected = Fixture.Create<Api.Application.Models.EmploymentCheck>();

            // Act
            await _sut.Insert(expected);

            // Assert
            var actual = (await GetAll<Api.Application.Models.EmploymentCheck>())
                .Single(x => x.CorrelationId == expected.CorrelationId);

            actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                );
            actual.MinDate.Should().BeCloseTo(expected.MinDate, TimeSpan.FromSeconds(1));
            actual.MaxDate.Should().BeCloseTo(expected.MaxDate, TimeSpan.FromSeconds(1));
            actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            actual.LastUpdatedOn.Should().BeCloseTo(expected.LastUpdatedOn, TimeSpan.FromSeconds(1));
        }
    }
}