using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.TestHelpers
{
    public static class LoggerExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> mockLogger, LogLevel logLevel, string message, Times times)
        {
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    It.IsAny<Exception?>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)
                ),
                times
            );
        }
    }
}