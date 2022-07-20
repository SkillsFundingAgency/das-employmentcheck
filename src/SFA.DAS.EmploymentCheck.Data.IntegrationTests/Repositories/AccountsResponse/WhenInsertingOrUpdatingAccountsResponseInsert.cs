using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

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

        [Test]
        public async Task CanInsertThenUpdate()
        {
            // Arrange
            _sut = new AccountsResponseRepository(Settings);
            var expected = Fixture.Build<Models.AccountsResponse>()
                .With(x => x.PayeSchemes, "First Value")
                .Create();
            await _sut.InsertOrUpdate(expected);
            expected.PayeSchemes = "Second Value";

            // Act
            await _sut.InsertOrUpdate(expected);


            // Assert
            _actual = await Get<Models.AccountsResponse>(expected.Id);

            _actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void CanCreate()
        {
            // Arrange
            var expected = Fixture.Create<Models.AccountsResponse>();

            // Act
            var actual = new Models.AccountsResponse(
                expected.Id,
                expected.ApprenticeEmploymentCheckId,
                expected.CorrelationId,
                expected.AccountId,
                expected.PayeSchemes,
                expected.HttpResponse,
                expected.HttpStatusCode,
                expected.LastUpdatedOn);
            actual.CreatedOn = expected.CreatedOn;

            // Assert
            actual.Should().BeEquivalentTo(expected);

        }
    }
}

