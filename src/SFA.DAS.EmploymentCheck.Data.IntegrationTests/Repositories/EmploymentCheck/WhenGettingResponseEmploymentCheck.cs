using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheck
{
    public class WhenGettingResponseEmploymentCheck
        : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;
        private Models.EmploymentCheck _actual;

        [Test]
        public async Task CanGet()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, null, null);
            var saved = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed)
                .Without(x => x.MessageSentDate)
                .Create();

            await Insert(saved);

            // Act
            _actual = await _sut.GetResponseEmploymentCheck();

            // Assert
            _actual.Should().NotBeNull();
            _actual.Id.Should().Be(saved.Id);
            _actual.CorrelationId.Should().Be(saved.CorrelationId);
            _actual.AccountId.Should().Be(saved.AccountId);
            _actual.Uln.Should().Be(saved.Uln);
            _actual.Employed.Should().Be(saved.Employed);
            _actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
            _actual.MessageSentDate.Should().NotBeNull();
        }
    }
}

