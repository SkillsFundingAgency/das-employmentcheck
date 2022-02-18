using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests
{
    public static class MockILoggerExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times)
        {
            logger.Verify(l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()),
                times);
        }
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times, string logMessage)
        {
            logger.Verify(m => m.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                times);
        }

        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times, string logMessage, Exception exception)
        {
            logger.Verify(m => m.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Equals(logMessage)),
                exception,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                times);
        }

        public static void VerifyLogContains<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times, string logMessage)
        {
            logger.Verify(m => m.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains(logMessage)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                times);
        }
    }
}
