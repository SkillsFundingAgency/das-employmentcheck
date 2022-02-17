using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.AccountsResponse
{
    public class WhenInsertingOrUpdatingAccountsResponseUpdate
        : RepositoryTestBase
    {
        private IAccountsResponseRepository _sut;
        private Models.AccountsResponse _actual;

        [Test]
        public async Task CanUpdate()
        {
            // Arrange
            _sut = new AccountsResponseRepository(Settings);
            var saved = Fixture.Create<Models.AccountsResponse>();
            await Insert(saved);

            var expected = Fixture.Build<Models.AccountsResponse>()
                .With(e => e.Id, saved.Id)
                .Create();

            // Act
            await _sut.InsertOrUpdate(expected);

            // Assert
            _actual = await Get<Models.AccountsResponse>(saved.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

