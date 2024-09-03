using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Controllers.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using EPR.Payment.Facade.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers
{
    [TestClass]
    public class PaymentsControllerTests
    {
        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_ValidRequest_ReturnsRedirectResponse(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            PaymentsController controller,
            PaymentRequestDto request,
            PaymentResponseDto expectedResponse)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            paymentsServiceMock.Setup(s => s.InitiatePaymentAsync(request, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.InitiatePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain($"window.location.href = '{expectedResponse.NextUrl}'");
            }
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithValidArguments_ShouldInitializeCorrectly(
            [Frozen] Mock<IPaymentsService> _paymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsController>> _loggerMock,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] PaymentServiceOptions _paymentServiceOptions)
        {
            // Arrange
            _paymentServiceOptionsMock.Setup(o => o.Value).Returns(_paymentServiceOptions);

            // Act
            var controller = new PaymentsController(
                _paymentsServiceMock.Object,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object
            );

            // Assert
            controller.Should().NotBeNull();
            controller.Should().BeAssignableTo<PaymentsController>();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullPaymentsService_ShouldThrowArgumentNullException(
            [Frozen] Mock<ILogger<PaymentsController>> _loggerMock,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock)
        {

            // Act
            Action act = () => new PaymentsController(
                null!,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("paymentsService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException(
            [Frozen] Mock<IPaymentsService> _paymentsServiceMock,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock)
        {

            // Act
            Action act = () => new PaymentsController(
                _paymentsServiceMock.Object,
                null!,
                _paymentServiceOptionsMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithPaymentServiceOptionsWithoutErrorUrl_ShouldThrowArgumentNullException(
            [Frozen] Mock<IPaymentsService> _paymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsController>> _loggerMock,
            [Frozen] Mock<IOptions<PaymentServiceOptions>> _paymentServiceOptionsMock,
            [Frozen] PaymentServiceOptions _paymentServiceOptions)
        {
            // Arrange
            _paymentServiceOptions.ErrorUrl = null!;
            _paymentServiceOptionsMock.Setup(x => x.Value).Returns(_paymentServiceOptions);

            // Act
            Action act = () => new PaymentsController(
                _paymentsServiceMock.Object,
                _loggerMock.Object,
                _paymentServiceOptionsMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("paymentServiceOptions");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_AmountZero_ReturnsBadRequest(PaymentsController controller)
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                UserId = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                Regulator = "Test Regulator",
                Reference = "Test Reference",
                Amount = 0 // Invalid amount
            };

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.InitiatePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("Amount must be greater than 0");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_AmountNegative_ReturnsBadRequest(PaymentsController controller)
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                UserId = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                Regulator = "Test Regulator",
                Reference = "Test Reference",
                Amount = -1 // Invalid amount
            };

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.InitiatePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("Amount must be greater than 0");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_NextUrlIsNull_ReturnsErrorUrl(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            [Frozen] Mock<ILogger<PaymentsController>> loggerMock,
            PaymentsController controller,
            PaymentRequestDto request,
            PaymentResponseDto expectedResponse)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            expectedResponse.NextUrl = null;
            paymentsServiceMock.Setup(s => s.InitiatePaymentAsync(request, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.InitiatePayment(request, cancellationToken);

            // Assert
            loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(LogMessages.NextUrlNull)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);

            using (new AssertionScope())
            {
                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain("window.location.href = 'https://example.com/error'");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_InvalidRequest_ReturnsBadRequest(
            PaymentsController controller)
        {
            // Arrange
            var request = new PaymentRequestDto(); // Invalid request
            controller.ModelState.AddModelError("Amount", "Amount is required");

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.InitiatePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<SerializableError>();
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_ThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            PaymentsController controller)
        {
            // Arrange
            var invalidRequest = new PaymentRequestDto();
            var validationException = new ValidationException("Validation error");
            var cancellationToken = new CancellationToken();

            paymentsServiceMock.Setup(s => s.InitiatePaymentAsync(invalidRequest, cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await controller.InitiatePayment(invalidRequest, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_ThrowsException_ReturnsErrorUrl(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            PaymentsController controller,
            PaymentRequestDto request)
        {
            // Arrange
            var exception = new Exception("Some error");
            var cancellationToken = new CancellationToken();
            var errorUrl = "https://example.com/error";

            paymentsServiceMock.Setup(s => s.InitiatePaymentAsync(request, cancellationToken)).ThrowsAsync(exception);

            // Act
            var result = await controller.InitiatePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain($"window.location.href = '{errorUrl}'");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_ValidExternalPaymentId_ReturnsOk(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            PaymentsController controller,
            Guid externalPaymentId,
            CompletePaymentResponseDto expectedResponse)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            paymentsServiceMock.Setup(s => s.CompletePaymentAsync(externalPaymentId, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.CompletePayment(externalPaymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
                paymentsServiceMock.Verify(s => s.CompletePaymentAsync(externalPaymentId, cancellationToken), Times.Once);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_EmptyExternalPaymentId_ReturnsBadRequest(
            PaymentsController controller)
        {
            // Arrange
            var externalPaymentId = Guid.Empty;
            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.CompletePayment(externalPaymentId, cancellationToken);

            // Assert
            using (var scope = new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                var problemDetails = badRequestResult?.Value as ProblemDetails;

                problemDetails.Should().NotBeNull();
                problemDetails?.Detail.Should().Be("ExternalPaymentId cannot be empty.");
                problemDetails?.Title.Should().Be("Validation Error");
                problemDetails?.Status.Should().Be(400);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_ThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            PaymentsController controller,
            Guid externalPaymentId)
        {
            // Arrange
            var validationException = new ValidationException("Validation error");
            var cancellationToken = new CancellationToken();

            paymentsServiceMock.Setup(s => s.CompletePaymentAsync(externalPaymentId, cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await controller.CompletePayment(externalPaymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_ThrowsException_ReturnsErrorUrl(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            PaymentsController controller,
            Guid externalPaymentId)
        {
            // Arrange
            var exception = new Exception("Some error");
            var cancellationToken = new CancellationToken();
            var errorUrl = "https://example.com/error";

            paymentsServiceMock.Setup(s => s.CompletePaymentAsync(externalPaymentId, cancellationToken)).ThrowsAsync(exception);

            // Act
            var result = await controller.CompletePayment(externalPaymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain($"window.location.href = '{errorUrl}'");
            }
        }
    }
}
