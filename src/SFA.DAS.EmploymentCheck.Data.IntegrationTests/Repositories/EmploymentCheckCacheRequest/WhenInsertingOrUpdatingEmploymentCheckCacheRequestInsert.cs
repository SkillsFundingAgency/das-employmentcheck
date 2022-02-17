using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;

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
            _sut = new EmploymentCheckCacheRequestRepository(Settings, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());
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

