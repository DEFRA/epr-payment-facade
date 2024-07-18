using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.UnitTests.TestHelpers;
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
        private IFixture fixture;

        [TestInitialize]
        public void Initialize()
        {
            fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Configure PaymentServiceOptions with valid values to avoid null references
            fixture.Customize<PaymentServiceOptions>(c => c.With(x => x.ReturnUrl, "https://example.com/return")
                                                           .With(x => x.Description, "Test Description")
                                                           .With(x => x.ErrorUrl, "https://example.com/error"));
        }

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
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain($"window.location.href = '{expectedResponse.NextUrl}'");
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

            var redirectResult = result as RedirectResult;
            redirectResult?.Url.Should().Be("https://example.com/error");
            redirectResult?.Permanent.Should().BeFalse();
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
            result.Should().BeOfType<BadRequestObjectResult>();
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
            using (new FluentAssertions.Execution.AssertionScope())
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
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().BeOfType<RedirectResult>();
                var redirectResult = result as RedirectResult;
                redirectResult?.Url.Should().Be(errorUrl);
                redirectResult?.Permanent.Should().BeFalse();
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_ValidGovPayPaymentId_ReturnsOk(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            PaymentsController controller,
            string govPayPaymentId,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.CompletePayment(govPayPaymentId, completeRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            paymentsServiceMock.Verify(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest, cancellationToken), Times.Once);
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_NullGovPayPaymentId_ReturnsBadRequest(
            PaymentsController controller,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.CompletePayment(null, completeRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("GovPayPaymentId cannot be null or empty");
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_EmptyGovPayPaymentId_ReturnsBadRequest(
            PaymentsController controller,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.CompletePayment(string.Empty, completeRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("GovPayPaymentId cannot be null or empty");
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_ThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            PaymentsController controller,
            string govPayPaymentId,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var validationException = new ValidationException("Validation error");
            var cancellationToken = new CancellationToken();

            paymentsServiceMock.Setup(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest, cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await controller.CompletePayment(govPayPaymentId, completeRequest, cancellationToken);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
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
            string govPayPaymentId,
            CompletePaymentRequestDto completeRequest)
        {
            // Arrange
            var exception = new Exception("Some error");
            var cancellationToken = new CancellationToken();
            var errorUrl = "https://example.com/error";

            paymentsServiceMock.Setup(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest, cancellationToken)).ThrowsAsync(exception);

            // Act
            var result = await controller.CompletePayment(govPayPaymentId, completeRequest, cancellationToken);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().BeOfType<RedirectResult>();
                var redirectResult = result as RedirectResult;
                redirectResult?.Url.Should().Be(errorUrl);
                redirectResult?.Permanent.Should().BeFalse();
            }
        }
    }
}
