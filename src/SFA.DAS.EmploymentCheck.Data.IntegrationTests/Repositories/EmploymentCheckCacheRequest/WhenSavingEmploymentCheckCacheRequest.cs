using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System.Linq;
using System.Threading.Tasks;

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

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

