using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.HmrcRepositories
{
    public class WhenGetHmrcRateLimiterOptions 
    {
        [Test]
        public void Then_GetTable_CloudTable_IsReturned()
        {
            //Arrange
            HmrcApiRateLimiterConfiguration options = new HmrcApiRateLimiterConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());

            //Act
            var result = sut.GetHmrcRateLimiterOptions();

            //Assert
            result.Should().NotBeNull();

        }

        [Test]
        public void Then_GetTable_CloudTable_And_Then_IncreaseDelaySetting()
        {
            //Arrange
            var rateLimiterOptions = new HmrcApiRateLimiterOptions();
            HmrcApiRateLimiterConfiguration options = new HmrcApiRateLimiterConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            var apiOptions = sut.GetHmrcRateLimiterOptions();

            //Act
            var result = sut.IncreaseDelaySetting(rateLimiterOptions);

            //Assert
            result.Should().NotBeNull();

        }

        [Test]
        public void Then_GetTable_CloudTable_And_Then_ReduceDelaySetting()
        {
            //Arrange
            var rateLimiterOptions = new HmrcApiRateLimiterOptions();
            HmrcApiRateLimiterConfiguration options = new HmrcApiRateLimiterConfiguration();
            IHmrcApiOptionsRepository sut = new HmrcApiOptionsRepository(options, Mock.Of<ILogger<HmrcApiOptionsRepository>>());
            var apiOptions = sut.GetHmrcRateLimiterOptions();

            //Act
            var result = sut.ReduceDelaySetting(rateLimiterOptions);

            //Assert
            result.Should().NotBeNull();

        }
    }
}
