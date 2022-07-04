using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.HmrcRepositories
{
    public class WhenGetHmrcRateLimiterOptions 
    {
        [Test]
        public async Task Then_GetTable_CloudTable_IsReturned()
        {
            //Arrange
            HmrcApiRateLimiterOptions options = new HmrcApiRateLimiterOptions();
            HmrcApiRateLimiterConfiguration config = new HmrcApiRateLimiterConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(config, Mock.Of<ILogger<HmrcApiOptionsRepository>>());

            //Act
            HmrcApiRateLimiterOptions result = await sut.GetHmrcRateLimiterOptions();

            //Assert
            result.Should().NotBeNull();
            result.DelayAdjustmentIntervalInMs.Should().Be(options.DelayAdjustmentIntervalInMs);
            result.MinimumUpdatePeriodInDays.Should().Be(options.MinimumUpdatePeriodInDays);
            result.TooManyRequestsRetryCount.Should().Be(options.TooManyRequestsRetryCount);
            result.TransientErrorRetryCount.Should().Be(options.TransientErrorRetryCount);
            result.TransientErrorDelayInMs.Should().Be(options.TransientErrorDelayInMs);
            result.TokenRetrievalRetryCount.Should().Be(options.TokenRetrievalRetryCount);
            result.TokenFailureRetryDelayInMs.Should().Be(options.TokenFailureRetryDelayInMs);

        }

        [Test]
        public async Task Then_GetTable_CloudTable_And_Then_IncreaseDelaySetting()
        {
            //Arrange
            HmrcApiRateLimiterConfiguration options = new HmrcApiRateLimiterConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            HmrcApiRateLimiterOptions result = await sut.GetHmrcRateLimiterOptions();
            int delayInMs = result.DelayInMs;

            //Act
            await sut.IncreaseDelaySetting(result);
            

            //Assert
            result.Should().NotBeNull();
            result.DelayInMs.Should().BeGreaterThan(delayInMs);

        }

        [Test]
        public async Task Then_GetTable_CloudTable_And_Then_ReduceDelaySetting()
        {
            //Arrange
            HmrcApiRateLimiterConfiguration options = new HmrcApiRateLimiterConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            HmrcApiRateLimiterOptions result = await sut.GetHmrcRateLimiterOptions();
            int delayInMs = result.DelayInMs;

            //Act
            await sut.ReduceDelaySetting(result);
            

            //Assert
            result.Should().NotBeNull();
            result.DelayInMs.Should().BeLessThanOrEqualTo(delayInMs);

        }

    }
}
