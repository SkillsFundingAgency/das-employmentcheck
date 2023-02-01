using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheckCacheRequest
{
    public class WhenInsertingOrUpdatingEmploymentCheckCacheRequestInsert
        : RepositoryTestBase
    {
        private IEmploymentCheckCacheRequestRepository _sut;
        private Models.EmploymentCheckCacheRequest _actual;

        [Test]
        public async Task CanInsert()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestRepository(Settings, Mock.Of<IHmrcApiOptionsRepository>(), Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());
            var expected = Fixture.Create<Models.EmploymentCheckCacheRequest>();

            // Act
            await _sut.Save(expected);


            // Assert
            _actual = (await GetAll<Models.EmploymentCheckCacheRequest>())
                .Single(x => x.Id == expected.Id);

            _actual.Should().BeEquivalentTo(expected);
        }
    }
}

