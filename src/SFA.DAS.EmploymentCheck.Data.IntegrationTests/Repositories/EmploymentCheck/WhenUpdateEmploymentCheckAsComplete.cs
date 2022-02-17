using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

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
                .Without(x => x.Employed)
                .Create();
            await Insert(check);

            var request = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(x => x.ApprenticeEmploymentCheckId, check.Id)
                .Create();

            // Act
            await UnitOfWorkInstance.BeginAsync();
            await _sut.UpdateEmploymentCheckAsComplete(request, UnitOfWorkInstance);
            await UnitOfWorkInstance.CommitAsync();

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            actual.Employed = request.Employed;
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task Then_updates_EmploymentCheck_record_where_Employed_Is_False()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short?)-1)
                .With(x => x.Employed, false)
                .Create();
            await Insert(check);

            var request = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(x => x.ApprenticeEmploymentCheckId, check.Id)
                .Create();

            // Act
            await UnitOfWorkInstance.BeginAsync();
            await _sut.UpdateEmploymentCheckAsComplete(request, UnitOfWorkInstance);
            await UnitOfWorkInstance.CommitAsync();

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            actual.Employed = request.Employed;
            actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
            actual.LastUpdatedOn?.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task Then_does_not_update_EmploymentCheck_record_where_Employed_Is_True()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());

            _expected = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short?)-1)
                .With(x => x.Employed, true)
                .Create();
            await Insert(_expected);

            var request = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(x => x.ApprenticeEmploymentCheckId, _expected.Id)
                .With(x => x.Employed, false)
                .Create();

            // Act
            await UnitOfWorkInstance.BeginAsync();
            await _sut.UpdateEmploymentCheckAsComplete(request, UnitOfWorkInstance);
            await UnitOfWorkInstance.CommitAsync();

            // Assert
            Get<Models.EmploymentCheck>(_expected.Id).Result.Should().BeEquivalentTo(_expected); // i.e. unchanged
        }
    }
}

