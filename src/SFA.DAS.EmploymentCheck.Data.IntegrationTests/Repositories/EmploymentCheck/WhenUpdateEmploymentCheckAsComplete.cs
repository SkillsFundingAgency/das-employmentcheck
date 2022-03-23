using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenUpdateEmploymentCheckAsComplete : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;
        private Models.EmploymentCheck _expected;

        [Test]
        public async Task Then_updates_EmploymentCheck_record_where_Employed_Is_Null()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short?)-1)
                .With(x =>x.ErrorType, "HmrcFailure")
                .Without(x => x.Employed)
                .Create();
            await Insert(check);

            // Act
            await UnitOfWorkInstance.BeginAsync();
            await _sut.UpdateEmploymentCheckAsComplete(check, UnitOfWorkInstance);
            await UnitOfWorkInstance.CommitAsync();

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            actual.Employed = check.Employed;
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().Be("HmrcFailure");
        }

        [Test]
        public async Task Then_updates_EmploymentCheck_record_where_Employed_Is_False()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short?)-1)
                .With(x => x.Employed, false)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(check);

            // Act
            await UnitOfWorkInstance.BeginAsync();
            await _sut.UpdateEmploymentCheckAsComplete(check, UnitOfWorkInstance);
            await UnitOfWorkInstance.CommitAsync();

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            actual.Employed = check.Employed;
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            actual.ErrorType.Should().BeNull();
        }

        [Test]
        public async Task Then_does_not_update_EmploymentCheck_record_where_Employed_Is_True()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());

            _expected = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short?)-1)
                .With(x => x.Employed, true)
                .Without(x => x.ErrorType)
                .Create();
            await Insert(_expected);

            // Act
            await UnitOfWorkInstance.BeginAsync();
            await _sut.UpdateEmploymentCheckAsComplete(_expected, UnitOfWorkInstance);
            await UnitOfWorkInstance.CommitAsync();

            // Assert
            Get<Models.EmploymentCheck>(_expected.Id).Result.Should().BeEquivalentTo(_expected); // i.e. unchanged
        }
    }
}

