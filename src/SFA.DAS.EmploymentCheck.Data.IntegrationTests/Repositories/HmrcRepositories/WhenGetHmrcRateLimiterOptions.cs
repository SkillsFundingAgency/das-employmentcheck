using System;
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
            var config = new AzureStorageConnectionConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(config, Mock.Of<ILogger<HmrcApiOptionsRepository>>());

            //Act
            HmrcApiRateLimiterOptions result = await sut.GetHmrcRateLimiterOptions();

            //Assert
            result.Should().NotBeNull();
            result.DelayAdjustmentIntervalInMs.Should().Be(options.DelayAdjustmentIntervalInMs);
            result.MinimumReduceDelayIntervalInMinutes.Should().Be(options.MinimumReduceDelayIntervalInMinutes);
            result.MinimumIncreaseDelayIntervalInSeconds.Should().Be(options.MinimumIncreaseDelayIntervalInSeconds);
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
            var options = new AzureStorageConnectionConfiguration();
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
        public async Task Then_IncreaseDelaySetting_Does_Nothing_when_Latest_Update_Less_then_MinimumIncreaseDelayIntervalInSeconds_Ago()
        {
            //Arrange
            var options = new AzureStorageConnectionConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            var result = await sut.GetHmrcRateLimiterOptions();
            var delayInMs = result.DelayInMs;
            result.MinimumIncreaseDelayIntervalInSeconds = 1;
            result.UpdateDateTime = DateTime.UtcNow;

            //Act
            await sut.IncreaseDelaySetting(result);

            //Assert
            result.Should().NotBeNull();
            result.DelayInMs.Should().Be(delayInMs);
        }
        
        [Test]
        public async Task Then_IncreaseDelaySetting_Updates_DelayInMs_when_Latest_Update_Greater_then_MinimumIncreaseDelayIntervalInSeconds_Ago()
        {
            //Arrange
            var options = new AzureStorageConnectionConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            var result = await sut.GetHmrcRateLimiterOptions();
            var delayInMs = result.DelayInMs;
            result.MinimumIncreaseDelayIntervalInSeconds = 1;
            result.UpdateDateTime = DateTime.UtcNow.AddSeconds(-2);

            //Act
            await sut.IncreaseDelaySetting(result);

            //Assert
            result.Should().NotBeNull();
            result.DelayInMs.Should().BeGreaterThan(delayInMs);
        }
        
        [Test]
        public async Task Then_IncreaseDelaySetting_When_LastUpdated_Is_Greater_then_MinimumIncreaseDelayIntervalInSeconds()
        {
            //Arrange
            var options = new AzureStorageConnectionConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            HmrcApiRateLimiterOptions result = await sut.GetHmrcRateLimiterOptions();
            
            result.MinimumIncreaseDelayIntervalInSeconds = 1;
            result.UpdateDateTime = DateTime.UtcNow.AddSeconds(-3);
            
            var delayInMs = result.DelayInMs;

            //Act
            await sut.IncreaseDelaySetting(result);
            

            //Assert
            result.Should().NotBeNull();
            result.DelayInMs.Should().BeGreaterThan(delayInMs);
        }

        [Test]
        public async Task Then_Does_Not_IncreaseDelaySetting_When_LastUpdated_Is_Less_then_MinimumIncreaseDelayIntervalInSeconds()
        {
            //Arrange
            var options = new AzureStorageConnectionConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            HmrcApiRateLimiterOptions result = await sut.GetHmrcRateLimiterOptions();
            
            result.MinimumIncreaseDelayIntervalInSeconds = 1;
            result.UpdateDateTime = DateTime.UtcNow.AddSeconds(2);
            
            var delayInMs = result.DelayInMs;

            //Act
            await sut.IncreaseDelaySetting(result);
            

            //Assert
            result.Should().NotBeNull();
            result.DelayInMs.Should().Be(delayInMs);
        }

        [Test]
        public async Task Then_GetTable_CloudTable_And_Then_ReduceDelaySetting()
        {
            //Arrange
            var options = new AzureStorageConnectionConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            HmrcApiRateLimiterOptions result = await sut.GetHmrcRateLimiterOptions();
            int delayInMs = result.DelayInMs;

            //Act
            await sut.ReduceDelaySetting(result);
            

            //Assert
            result.Should().NotBeNull();
            result.DelayInMs.Should().BeLessThanOrEqualTo(delayInMs);

        }

        [Test]
        public async Task Then_retry_delay_time_is_Not_decreased_When_DelayInMs_Is_Already_Zero()
        {
            // Arrange
            var options = new AzureStorageConnectionConfiguration();
            var hmrcApiOptionsRepositoryLoggerMock = new Mock<ILogger<HmrcApiOptionsRepository>>();

            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, hmrcApiOptionsRepositoryLoggerMock.Object);
            var result = await sut.GetHmrcRateLimiterOptions();
            
            result.DelayInMs = 0;
            result.UpdateDateTime = DateTime.UtcNow;
            
            // Act
            await sut.ReduceDelaySetting(result);

            // Assert
            hmrcApiOptionsRepositoryLoggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Test]
        public async Task Then_retry_delay_time_is_decreased_When_DelayInMs_Is_Greater_than_Zero()
        {
            // Arrange
            var options = new AzureStorageConnectionConfiguration();
            var hmrcApiOptionsRepositoryLoggerMock = new Mock<ILogger<HmrcApiOptionsRepository>>();

            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, hmrcApiOptionsRepositoryLoggerMock.Object);
            var result = await sut.GetHmrcRateLimiterOptions();
            
            result.DelayInMs = 100;
            result.UpdateDateTime = DateTime.UtcNow.AddMinutes(-6);
            
            // Act
            await sut.ReduceDelaySetting(result);

            // Assert
            hmrcApiOptionsRepositoryLoggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Test]
        public async Task Then_retry_delay_time_is_Not_decreased_When_When_LastUpdated_Is_Less_then_MinimumReduceDelayIntervalInMinutes()
        {
            // Arrange
            var options = new AzureStorageConnectionConfiguration();
            var hmrcApiOptionsRepositoryLoggerMock = new Mock<ILogger<HmrcApiOptionsRepository>>();

            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, hmrcApiOptionsRepositoryLoggerMock.Object);
            var result = await sut.GetHmrcRateLimiterOptions();
            
            result.DelayInMs = 100;
            result.UpdateDateTime = DateTime.UtcNow;
            
            // Act
            await sut.ReduceDelaySetting(result);

            // Assert
            hmrcApiOptionsRepositoryLoggerMock.Verify(
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
