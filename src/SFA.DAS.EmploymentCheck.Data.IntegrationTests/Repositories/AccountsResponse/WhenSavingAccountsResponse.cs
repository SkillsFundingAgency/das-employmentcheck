using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.AccountsResponse
{
    public class WhenSavingAccountsResponse
        : RepositoryTestBase
    {
        private IAccountsResponseRepository _sut;
        private Models.AccountsResponse _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new AccountsResponseRepository(Settings);
            var expected = Fixture.Create<Models.AccountsResponse>();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = await Get<Models.AccountsResponse>(expected.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

