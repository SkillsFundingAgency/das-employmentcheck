using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.DataCollectionsApiClientTests
{
    public class WhenUsingApiOptionRepositoryTests
    {
        [Test]
        public async Task Then_GetTable_CloudTableIsReturned()
        {
            //Arrange
            AzureStorageConnectionConfiguration config = new AzureStorageConnectionConfiguration();
            IApiOptionsRepository sut = new ApiOptionsRepository(config);
            ApiRetryOptions options = new ApiRetryOptions();

            //Act
            var result = await sut.GetOptions();

            //Assert
            result.Should().NotBeNull();
            result.TooManyRequestsRetryCount.Should().Be(options.TooManyRequestsRetryCount);
            result.TransientErrorRetryCount.Should().Be(options.TransientErrorRetryCount);
            result.TransientErrorDelayInMs.Should().Be(options.TransientErrorDelayInMs);
            result.TokenRetrievalRetryCount.Should().Be(options.TokenRetrievalRetryCount);
        }
    }
}
