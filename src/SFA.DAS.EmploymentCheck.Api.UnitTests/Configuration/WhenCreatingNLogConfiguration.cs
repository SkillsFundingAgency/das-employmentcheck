using FluentAssertions;
using NLog;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Microsoft.ApplicationInsights.NLogTarget;
using NLog.Targets;
using SFA.DAS.NLog.Targets.Redis.DotNetCore;

namespace SFA.DAS.EmploymentCheck.Api.UnitTests.Configuration
{
    public class WhenCreatingNLogConfiguration
    {
        private const string AppName = "das-employment-check-api";

        [Test]
        public void Then_LocalFileTarget_Is_Created_For_Local_Environment()
        {
            // Arrange
            Environment.SetEnvironmentVariable("EnvironmentName", "LOCAL");
            var configuration = new NLogConfiguration();
            var expected = new FileTarget("Disk")
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), $"logs\\{AppName}.${{shortdate}}.log"),
                Layout = "${longdate} [${uppercase:${level}}] [${logger}] - ${message} ${onexception:${exception:format=tostring}}"
            };

            // Act
            configuration.ConfigureNLog();

            // Assert
            LogManager.Configuration.AllTargets.Should().HaveCount(1);
            LogManager.Configuration.AllTargets.First().Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Then_RedisTarget_Is_Created_For_NonLocal_Environment()
        {
            // Arrange
            Environment.SetEnvironmentVariable("EnvironmentName", "TEST");
            var configuration = new NLogConfiguration();
            var expected = new RedisTarget
            {
                Name = "RedisLog",
                AppName = AppName,
                EnvironmentKeyName = "EnvironmentName",
                ConnectionStringName = "LoggingRedisConnectionString",
                IncludeAllProperties = true,
                Layout = "${message}"
            };

            // Act
            configuration.ConfigureNLog();

            // Assert
            LogManager.Configuration.AllTargets.Should().HaveCount(2);
            LogManager.Configuration.AllTargets.First().Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Then_ApplicationInsightsTarget_Is_Created_For_NonLocal_Environment()
        {
            // Arrange
            Environment.SetEnvironmentVariable("EnvironmentName", "TEST");
            var configuration = new NLogConfiguration();
            var expected = new ApplicationInsightsTarget
            {
                Name = "AppInsightsLog"
            };

            // Act
            configuration.ConfigureNLog();

            // Assert
            LogManager.Configuration.AllTargets.Should().HaveCount(2);
            LogManager.Configuration.AllTargets.Last().Should().BeEquivalentTo(expected);
        }
    }
}
