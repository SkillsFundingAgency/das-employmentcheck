using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.AccountsResponse
{
    public class WhenInsertingOrUpdatingAccountsResponseInsert
        : RepositoryTestBase
    {
        private IAccountsResponseRepository _sut;
        private Models.AccountsResponse _actual;

        [Test]
        public async Task CanInsert()
        {
            // Arrange
            _sut = new AccountsResponseRepository(Settings);
            var expected = Fixture.Create<Models.AccountsResponse>();

            // Act
            await _sut.InsertOrUpdate(expected);

            // Assert
            _actual = await Get<Models.AccountsResponse>(expected.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

