using FluentAssertions;
using NLog;
using NUnit.Framework;
using System;

namespace SFA.DAS.EmploymentCheck.Api.UnitTests.Configuration
{
    public class WhenCreatingNLogConfiguration
    {
        [Test]
        public void Then_Create_NLogConfiguration()
        {
            // Arrange
            var configuration = new NLogConfiguration();
            
            // Act
            configuration.ConfigureNLog();
            
            // Assert
            LogManager.Configuration.Should().NotBeNull();



        }
        [Test]
        public void Then_Create_With_Environment_Variable_TEST_NLogConfiguration()
        {
            // Arrange
            var configuration = new NLogConfiguration();
            Environment.SetEnvironmentVariable("EnvironmentName", "TEST");

            // Act
            configuration.ConfigureNLog();
            Environment.SetEnvironmentVariable("EnvironmentName", "LOCAL");

            // Assert
            LogManager.Configuration.Should().NotBeNull();
        }

    }
}
