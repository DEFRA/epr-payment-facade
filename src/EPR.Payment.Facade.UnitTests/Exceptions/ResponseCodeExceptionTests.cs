using FluentAssertions;
using System.Net;

namespace EPR.Payment.Facade.Common.Exceptions.Tests
{
    [TestClass]
    public class ResponseCodeExceptionTests
    {
        [TestMethod]
        public void Constructor_WithStatusCodeAndMessage_SetsPropertiesCorrectly()
        {
            // Arrange
            var statusCode = HttpStatusCode.BadRequest;
            var message = "An error occurred.";

            // Act
            var exception = new ResponseCodeException(statusCode, message);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                exception.StatusCode.Should().Be(statusCode);
                exception.Message.Should().Be(message);
            }
        }

        [TestMethod]
        public void Constructor_WithStatusCode_SetsStatusCodeCorrectly()
        {
            // Arrange
            var statusCode = HttpStatusCode.NotFound;

            // Act
            var exception = new ResponseCodeException(statusCode);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                exception.StatusCode.Should().Be(statusCode);
                exception.Message.Should().Be("Exception of type 'EPR.Payment.Facade.Common.Exceptions.ResponseCodeException' was thrown.");
            }
        }
    }
}
