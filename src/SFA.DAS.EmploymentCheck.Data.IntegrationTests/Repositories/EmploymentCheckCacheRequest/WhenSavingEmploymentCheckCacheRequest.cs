using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheckCacheRequest
{
    public class WhenSavingEmploymentCheckCacheRequest
         : RepositoryTestBase
    {
        private IEmploymentCheckCacheRequestRepository _sut;
        private Models.EmploymentCheckCacheRequest _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestRepository(Settings, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());
            var expected = Fixture.Create<Models.EmploymentCheckCacheRequest>();
            expected.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;

            // Act
            await _sut.Insert(expected);

            // Assert
            _actual = (await GetAll<Models.EmploymentCheckCacheRequest>())
                .Single(x => x.Id == expected.Id);

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                );

            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeCloseTo(expected.LastUpdatedOn.Value, TimeSpan.FromSeconds(1));
            _actual.MinDate.Should().BeCloseTo(expected.MinDate, TimeSpan.FromMilliseconds(100));
            _actual.MaxDate.Should().BeCloseTo(expected.MaxDate, TimeSpan.FromMilliseconds(100));
            _actual.Id.Should().BeGreaterThan(0);
            _actual.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }
    }
}

