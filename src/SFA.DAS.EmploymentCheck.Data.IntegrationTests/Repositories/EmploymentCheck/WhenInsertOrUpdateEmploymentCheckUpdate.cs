using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenInsertOrUpdateEmploymentCheckUpdate
        : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;
        private Models.EmploymentCheck _actual;

        [Test]
        public async Task CanUpdate()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var saved = Fixture.Create<Models.EmploymentCheck>();
            await Insert(saved);

            var expected = Fixture.Build<Models.EmploymentCheck>()
                .With(e => e.Id, saved.Id)
                .Create();

            // Act
            await _sut.InsertOrUpdate(expected);

            // Assert
            _actual = await Get<Models.EmploymentCheck>(saved.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

