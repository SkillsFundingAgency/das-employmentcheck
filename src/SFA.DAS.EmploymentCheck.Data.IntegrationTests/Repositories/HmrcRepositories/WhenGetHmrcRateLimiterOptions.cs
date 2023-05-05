using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.HmrcRepositories
{
    public class WhenGetHmrcRateLimiterOptions 
    {
        [Test]
        public void Then_retry_options_are_retrieved_from_config()
        {
            //Arrange
            HmrcApiRateLimiterOptions options = new HmrcApiRateLimiterOptions();
            var hmrcApiRateLimiterOptions = new Mock<IOptions<HmrcApiRateLimiterOptions>>();
            hmrcApiRateLimiterOptions.Setup(x => x.Value).Returns(options);
            var apiRetryDelaySettings = new ApiRetryDelaySettings();
            
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(apiRetryDelaySettings, hmrcApiRateLimiterOptions.Object, Mock.Of<ILogger<HmrcApiOptionsRepository>>());

            //Act
            HmrcApiRateLimiterOptions result = sut.GetHmrcRateLimiterOptions();

            //Assert
            result.Should().NotBeNull();
            result.DelayAdjustmentIntervalInMs.Should().Be(options.DelayAdjustmentIntervalInMs);
            result.MinimumReduceDelayIntervalInMinutes.Should().Be(options.MinimumReduceDelayIntervalInMinutes);
            result.MinimumIncreaseDelayIntervalInSeconds.Should().Be(options.MinimumIncreaseDelayIntervalInSeconds);
            result.TokenFailureRetryDelayInMs.Should().Be(options.TokenFailureRetryDelayInMs);

        }

        [Test]
        public void Then_IncreaseDelaySetting()
        {
            //Arrange
            var apiRetryDelaySettings = new ApiRetryDelaySettings 
            {
                DelayInMs = 50, 
                UpdateDateTime = DateTime.UtcNow.AddSeconds(-15)
            };
            HmrcApiRateLimiterOptions options = new HmrcApiRateLimiterOptions 
            {
                DelayAdjustmentIntervalInMs = 50,
                MinimumIncreaseDelayIntervalInSeconds = 10
            };
            var hmrcApiRateLimiterOptions = new Mock<IOptions<HmrcApiRateLimiterOptions>>();
            hmrcApiRateLimiterOptions.Setup(x => x.Value).Returns(options);
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(apiRetryDelaySettings, hmrcApiRateLimiterOptions.Object, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            HmrcApiRateLimiterOptions result = sut.GetHmrcRateLimiterOptions();
            var originalDelayInMs = apiRetryDelaySettings.DelayInMs;

            //Act
            var updatedSettings = sut.IncreaseDelaySetting(result);

            //Assert
            updatedSettings.DelayInMs.Should().BeGreaterThan(originalDelayInMs);
        }

        [Test]
        public void Then_IncreaseDelaySetting_Does_Nothing_when_Latest_Update_Less_then_MinimumIncreaseDelayIntervalInSeconds_Ago()
        {
            //Arrange
            var apiRetryDelaySettings = new ApiRetryDelaySettings
            {
                DelayInMs = 50,
                UpdateDateTime = DateTime.UtcNow.AddSeconds(-5)
            };
            HmrcApiRateLimiterOptions options = new HmrcApiRateLimiterOptions
            {
                DelayAdjustmentIntervalInMs = 50,
                MinimumIncreaseDelayIntervalInSeconds = 10
            };
            var hmrcApiRateLimiterOptions = new Mock<IOptions<HmrcApiRateLimiterOptions>>();
            hmrcApiRateLimiterOptions.Setup(x => x.Value).Returns(options);
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(apiRetryDelaySettings, hmrcApiRateLimiterOptions.Object, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            HmrcApiRateLimiterOptions result = sut.GetHmrcRateLimiterOptions();
            var originalDelayInMs = apiRetryDelaySettings.DelayInMs;

            //Act
            var updatedSettings = sut.IncreaseDelaySetting(result);

            //Assert
            updatedSettings.DelayInMs.Should().Be(originalDelayInMs);
        }

        [Test]
        public void Then_ReduceDelaySetting()
        {
            //Arrange
            var apiRetryDelaySettings = new ApiRetryDelaySettings
            {
                DelayInMs = 50,
                UpdateDateTime = DateTime.UtcNow.AddSeconds(-15)
            };
            HmrcApiRateLimiterOptions options = new HmrcApiRateLimiterOptions
            {
                DelayAdjustmentIntervalInMs = 50,
                MinimumIncreaseDelayIntervalInSeconds = 10
            };
            var hmrcApiRateLimiterOptions = new Mock<IOptions<HmrcApiRateLimiterOptions>>();
            hmrcApiRateLimiterOptions.Setup(x => x.Value).Returns(options);
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(apiRetryDelaySettings, hmrcApiRateLimiterOptions.Object, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            HmrcApiRateLimiterOptions result = sut.GetHmrcRateLimiterOptions();
            var originalDelayInMs = apiRetryDelaySettings.DelayInMs;

            //Act
            var updatedSettings = sut.ReduceDelaySetting(result);

            //Assert
            updatedSettings.DelayInMs.Should().BeLessThanOrEqualTo(originalDelayInMs);
        }

        [Test]
        public void Then_retry_delay_time_is_Not_decreased_When_DelayInMs_Is_Already_Zero()
        {
            //Arrange
            var apiRetryDelaySettings = new ApiRetryDelaySettings
            {
                DelayInMs = 0,
                UpdateDateTime = DateTime.UtcNow
            };
            HmrcApiRateLimiterOptions options = new HmrcApiRateLimiterOptions
            {
                DelayAdjustmentIntervalInMs = 50,
                MinimumIncreaseDelayIntervalInSeconds = 10
            };
            var hmrcApiRateLimiterOptions = new Mock<IOptions<HmrcApiRateLimiterOptions>>();
            hmrcApiRateLimiterOptions.Setup(x => x.Value).Returns(options);
            var hmrcApiOptionsRepositoryLogger = new Mock<ILogger<HmrcApiOptionsRepository>>();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(apiRetryDelaySettings, hmrcApiRateLimiterOptions.Object, hmrcApiOptionsRepositoryLogger.Object);
            HmrcApiRateLimiterOptions result = sut.GetHmrcRateLimiterOptions();

            // Act
            sut.ReduceDelaySetting(result);

            // Assert
            hmrcApiOptionsRepositoryLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Test]
        public void Then_retry_delay_time_is_decreased_When_DelayInMs_Is_Greater_than_Zero()
        {
            //Arrange
            var apiRetryDelaySettings = new ApiRetryDelaySettings
            {
                DelayInMs = 100,
                UpdateDateTime = DateTime.UtcNow.AddMinutes(-6)
            };
            HmrcApiRateLimiterOptions options = new HmrcApiRateLimiterOptions
            {
                DelayAdjustmentIntervalInMs = 50,
                MinimumIncreaseDelayIntervalInSeconds = 10
            };
            var hmrcApiRateLimiterOptions = new Mock<IOptions<HmrcApiRateLimiterOptions>>();
            hmrcApiRateLimiterOptions.Setup(x => x.Value).Returns(options);
            var hmrcApiOptionsRepositoryLogger = new Mock<ILogger<HmrcApiOptionsRepository>>();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(apiRetryDelaySettings, hmrcApiRateLimiterOptions.Object, hmrcApiOptionsRepositoryLogger.Object);
            HmrcApiRateLimiterOptions result = sut.GetHmrcRateLimiterOptions();

            // Act
            sut.ReduceDelaySetting(result);

            // Assert
            hmrcApiOptionsRepositoryLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Test]
        public void Then_retry_delay_time_is_Not_decreased_When_When_LastUpdated_Is_Less_then_MinimumReduceDelayIntervalInMinutes()
        {
            //Arrange
            var apiRetryDelaySettings = new ApiRetryDelaySettings
            {
                DelayInMs = 100,
                UpdateDateTime = DateTime.UtcNow.AddSeconds(-1)
            };
            HmrcApiRateLimiterOptions options = new HmrcApiRateLimiterOptions
            {
                DelayAdjustmentIntervalInMs = 50,
                MinimumReduceDelayIntervalInMinutes = 5
            };
            var hmrcApiRateLimiterOptions = new Mock<IOptions<HmrcApiRateLimiterOptions>>();
            hmrcApiRateLimiterOptions.Setup(x => x.Value).Returns(options);
            var hmrcApiOptionsRepositoryLogger = new Mock<ILogger<HmrcApiOptionsRepository>>();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(apiRetryDelaySettings, hmrcApiRateLimiterOptions.Object, hmrcApiOptionsRepositoryLogger.Object);
            HmrcApiRateLimiterOptions result = sut.GetHmrcRateLimiterOptions();

            // Act
            sut.ReduceDelaySetting(result);

            // Assert
            hmrcApiOptionsRepositoryLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }
    }
}
