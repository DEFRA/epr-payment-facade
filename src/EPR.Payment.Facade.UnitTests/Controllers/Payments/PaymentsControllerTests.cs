using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.UnitTests.Controllers
{
    [TestClass]
    public class PaymentsControllerTests
    {
        private Mock<IPaymentsService>? _paymentsServiceMock;
        private Mock<ILogger<PaymentsController>>? _loggerMock;
        private PaymentsController? _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _paymentsServiceMock = new Mock<IPaymentsService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();
            _controller = new PaymentsController(_paymentsServiceMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task InitiatePayment_ValidRequest_ReturnsRedirectResponse()
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Amount = 100,
                Reference = "REF123",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "Reg123"
            };
            var expectedResponse = new PaymentResponseDto
            {
                NextUrl = "https://example.com/response"
            };

            var cancellationToken = new CancellationToken();

            _paymentsServiceMock?.Setup(s => s.InitiatePaymentAsync(request, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller!.InitiatePayment(request, cancellationToken);

            // Assert
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            contentResult?.ContentType.Should().Be("text/html");
            contentResult?.Content.Should().Contain("window.location.href = 'https://example.com/response'");
        }

        [TestMethod]
        public async Task InitiatePayment_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaymentRequestDto(); // Invalid request
            _controller!.ModelState.AddModelError("Amount", "Amount is required");

            var cancellationToken = new CancellationToken();

            // Act
            var result = await _controller.InitiatePayment(request, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task InitiatePayment_ThrowsValidationException_ReturnsBadRequest()
        {
            // Arrange
            var invalidRequest = new PaymentRequestDto
            {
                // Missing required fields to make the model invalid
                // Amount is missing
                // Reference is missing
                // OrganisationId is missing
                // UserId is missing
                // Regulator is missing
            };

            var validationException = new ValidationException("Validation error");
            var cancellationToken = new CancellationToken();

            _paymentsServiceMock?.Setup(s => s.InitiatePaymentAsync(invalidRequest, cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await _controller!.InitiatePayment(invalidRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
        }

        [TestMethod]
        public async Task InitiatePayment_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Amount = 100,
                Reference = "REF123",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "Reg123"
            };
            var exception = new Exception("Some error");
            var cancellationToken = new CancellationToken();

            _paymentsServiceMock?.Setup(s => s.InitiatePaymentAsync(request, cancellationToken)).ThrowsAsync(exception);

            // Act
            var result = await _controller!.InitiatePayment(request, cancellationToken);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            objectResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(exception.Message);
        }

        [TestMethod]
        public async Task CompletePayment_ValidGovPayPaymentId_ReturnsOk()
        {
            // Arrange
            var govPayPaymentId = "12345";
            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _controller!.CompletePayment(govPayPaymentId, completeRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            _paymentsServiceMock?.Verify(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest, cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task CompletePayment_NullGovPayPaymentId_ReturnsBadRequest()
        {
            // Arrange
            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _controller!.CompletePayment(null, completeRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("GovPayPaymentId cannot be null or empty");
        }

        [TestMethod]
        public async Task CompletePayment_EmptyGovPayPaymentId_ReturnsBadRequest()
        {
            // Arrange
            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _controller!.CompletePayment(string.Empty, completeRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("GovPayPaymentId cannot be null or empty");
        }

        [TestMethod]
        public async Task CompletePayment_ThrowsValidationException_ReturnsBadRequest()
        {
            // Arrange
            var govPayPaymentId = "12345";
            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };
            var validationException = new ValidationException("Validation error");
            var cancellationToken = new CancellationToken();

            _paymentsServiceMock?.Setup(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest, cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await _controller!.CompletePayment(govPayPaymentId, completeRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
        }

        [TestMethod]
        public async Task CompletePayment_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var govPayPaymentId = "12345";
            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };
            var exception = new Exception("Some error");
            var cancellationToken = new CancellationToken();

            _paymentsServiceMock?.Setup(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest, cancellationToken)).ThrowsAsync(exception);

            // Act
            var result = await _controller!.CompletePayment(govPayPaymentId, completeRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            objectResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(exception.Message);
        }
    }
}
