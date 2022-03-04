using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheck
{
    public class WhenInsertOrUpdateEmploymentCheckInsert
        : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;
        private Models.EmploymentCheck _actual;

        [Test]
        public async Task CanInsert()
        {
            // Arrange
            var errorType = "NinoNotFound";
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var expected = Fixture.Build<Models.EmploymentCheck>()
               .With(x => x.ErrorType, errorType)
               .Without(x => x.Id)
               .Without(x => x.LastUpdatedOn)
               .Create();

            // Act
            await _sut.InsertOrUpdate(expected);

            // Assert
            _actual = await Get<Models.EmploymentCheck>(expected.Id);

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                );

            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeNull();
            _actual.MinDate.Should().BeCloseTo(expected.MinDate, TimeSpan.FromSeconds(1));
            _actual.MaxDate.Should().BeCloseTo(expected.MaxDate, TimeSpan.FromSeconds(1));
            _actual.Id.Should().BeGreaterThan(0);
            _actual.ErrorType.Should().Be(errorType);
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }
    }
}

