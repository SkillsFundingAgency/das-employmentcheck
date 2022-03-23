using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenGettingResponseEmploymentCheck
        : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;
        private Models.EmploymentCheck _actual;
        private Models.EmploymentCheck _expected;

        [SetUp]
        public async Task Setup()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, null, null);

            await Insert(Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Skipped)
                .Without(x => x.MessageSentDate)
                .Create());
            await Insert(Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Started)
                .Without(x => x.MessageSentDate)
                .Create());
            await Insert(Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed)
                .Create());

            _expected = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed)
                .Without(x => x.MessageSentDate)
                .Create();

            await Insert(_expected);
        }

        [Test]
        public async Task Then_unsent_completed_check_is_returned()
        {
            // Act
            _actual = await _sut.GetResponseEmploymentCheck();

            // Assert
            _actual.Should().BeEquivalentTo(_expected,
                opts => opts.Excluding(check => check.MessageSentDate));
        }

        [Test]
        public async Task Then_returned_check_is_marked_as_sent()
        {
            // Act
            _actual = await _sut.GetResponseEmploymentCheck();

            // Assert
            _actual.MessageSentDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }
    }
}

