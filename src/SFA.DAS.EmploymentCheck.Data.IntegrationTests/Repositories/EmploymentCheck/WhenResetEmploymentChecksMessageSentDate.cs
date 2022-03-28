using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenResetEmploymentChecksMessageSentDate : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;

        [Test]
        public async Task Then_updates_EmploymentCheck_By_CorrelationId_When_Status_Is_Completed()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 24);
            var correlationId = Guid.NewGuid();

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.CorrelationId, correlationId)
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(correlationId);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(1);
            actual.CorrelationId.Should().Be(correlationId);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().BeNull();
        }

        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_By_CorrelationId_When_Status_Is_Started()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 24);
            var correlationId = Guid.NewGuid();

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.CorrelationId, correlationId)
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Started)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(correlationId);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.CorrelationId.Should().Be(correlationId);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Started);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }

        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_By_CorrelationId_When_Status_Is_Skipped()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 24);
            var correlationId = Guid.NewGuid();

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.CorrelationId, correlationId)
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Skipped)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(correlationId);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.CorrelationId.Should().Be(correlationId);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Skipped);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }

        [Test]
        public async Task Then_updates_EmploymentCheck_records_where_MessageSentDate_Is_Between_the_Given_Dates_When_Status_Is_Completed()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 24);
            var messageSentFromDate = new DateTime(2022, 3, 23);
            var messageSentToDate = new DateTime(2022, 3, 26);

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x =>x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(1);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().BeNull();
        }

        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_records_where_MessageSentDate_Is_Before_the_Given_Dates_When_Status_Is_Completed()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 21);
            var messageSentFromDate = new DateTime(2022, 3, 22);
            var messageSentToDate = new DateTime(2022, 3, 26);

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }

        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_records_where_MessageSentDate_Is_After_the_Given_Dates_When_Status_Is_Completed()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 27);
            var messageSentFromDate = new DateTime(2022, 3, 22);
            var messageSentToDate = new DateTime(2022, 3, 26);

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }

        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_records_where_MessageSentDate_Is_Between_the_Given_Dates_When_Status_Is_Started()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 24);
            var messageSentFromDate = new DateTime(2022, 3, 23);
            var messageSentToDate = new DateTime(2022, 3, 26);

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Started)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Started);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }

        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_records_where_MessageSentDate_Is_Before_the_Given_Dates_When_Status_Is_Started()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 21);
            var messageSentFromDate = new DateTime(2022, 3, 23);
            var messageSentToDate = new DateTime(2022, 3, 26);

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Started)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Started);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }


        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_records_where_MessageSentDate_Is_After_the_Given_Dates_When_Status_Is_Started()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 27);
            var messageSentFromDate = new DateTime(2022, 3, 23);
            var messageSentToDate = new DateTime(2022, 3, 26);

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Started)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Started);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }

        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_records_where_MessageSentDate_Is_Between_the_Given_Dates_When_Status_Is_Skipped()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 24);
            var messageSentFromDate = new DateTime(2022, 3, 23);
            var messageSentToDate = new DateTime(2022, 3, 26);

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Skipped)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Skipped);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }

        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_records_where_MessageSentDate_Is_Before_the_Given_Dates_When_Status_Is_Skipped()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 21);
            var messageSentFromDate = new DateTime(2022, 3, 23);
            var messageSentToDate = new DateTime(2022, 3, 26);

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Skipped)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Skipped);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }

        [Test]
        public async Task Then_Does_Not_Update_EmploymentCheck_records_where_MessageSentDate_Is_After_the_Given_Dates_When_Status_Is_Skipped()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());
            var messageSentDate = new DateTime(2022, 3, 27);
            var messageSentFromDate = new DateTime(2022, 3, 23);
            var messageSentToDate = new DateTime(2022, 3, 26);

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Skipped)
                .With(x => x.Employed, true)
                .With(x => x.MessageSentDate, messageSentDate)
                .With(x => x.LastUpdatedOn, DateTime.Now)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            result.Should().Be(0);
            actual.Employed.Should().Be(check.Employed);
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Skipped);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
            actual.MessageSentDate.Should().Be(check.MessageSentDate);
        }
    }
}

