using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
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
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var expected = Fixture.Build<Models.EmploymentCheck>()
                .Without(x => x.Id)
                .Without(x => x.LastUpdatedOn)
                .Create();

            // Act
            await _sut.InsertOrUpdate(expected);

            // Assert
            _actual = await Get<Models.EmploymentCheck>(expected.Id);
            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

