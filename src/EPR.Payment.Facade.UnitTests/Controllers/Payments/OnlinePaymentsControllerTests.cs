using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.Common;
using EPR.Payment.Facade.Common.Enums.Payments;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
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
    public class OnlinePaymentsControllerTests
    {
        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_ValidRequest_ReturnsRedirectResponse(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Greedy] OnlinePaymentsController controller,
            [Frozen] OnlinePaymentRequestDto request,
            [Frozen] OnlinePaymentResponseDto expectedResponse)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            onlinePaymentsServiceMock.Setup(s => s.InitiateOnlinePaymentAsync(request, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            IActionResult result = await controller.InitiateOnlinePayment(request, cancellationToken);

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
            [Frozen] Mock<IOnlinePaymentsService> _onlinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OnlinePaymentsController>> _loggerMock,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock,
            [Frozen] OnlinePaymentServiceOptions _onlinePaymentServiceOptions)
        {
            // Arrange
            _onlinePaymentServiceOptionsMock.Setup(o => o.Value).Returns(_onlinePaymentServiceOptions);

            // Act
            var controller = new OnlinePaymentsController(
                _onlinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _onlinePaymentServiceOptionsMock.Object
            );

            // Assert
            controller.Should().NotBeNull();
            controller.Should().BeAssignableTo<OnlinePaymentsController>();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullOnlinePaymentsService_ShouldThrowArgumentNullException(
            [Frozen] Mock<ILogger<OnlinePaymentsController>> _loggerMock,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock)
        {

            // Act
            Action act = () => new OnlinePaymentsController(
                null!,
                _loggerMock.Object,
                _onlinePaymentServiceOptionsMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("onlinePaymentsService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException(
            [Frozen] Mock<IOnlinePaymentsService> _onlinePaymentsServiceMock,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock)
        {

            // Act
            Action act = () => new OnlinePaymentsController(
                _onlinePaymentsServiceMock.Object,
                null!,
                _onlinePaymentServiceOptionsMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithOnlinePaymentServiceOptionsWithoutErrorUrl_ShouldThrowArgumentNullException(
            [Frozen] Mock<IOnlinePaymentsService> _onlinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OnlinePaymentsController>> _loggerMock,
            [Frozen] Mock<IOptions<OnlinePaymentServiceOptions>> _onlinePaymentServiceOptionsMock,
            [Frozen] OnlinePaymentServiceOptions _onlinePaymentServiceOptions)
        {
            // Arrange
            _onlinePaymentServiceOptions.ErrorUrl = null!;
            _onlinePaymentServiceOptionsMock.Setup(x => x.Value).Returns(_onlinePaymentServiceOptions);

            // Act
            Action act = () => new OnlinePaymentsController(
                _onlinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _onlinePaymentServiceOptionsMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("onlinePaymentServiceOptions");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_AmountZero_ReturnsBadRequest([Greedy] OnlinePaymentsController controller)
        {
            // Arrange
            var request = new OnlinePaymentRequestDto
            {
                UserId = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                Regulator = "Test Regulator",
                Reference = "Test Reference",
                Amount = 0, // Invalid amount
                Description = PaymentDescConstants.RegistrationFee
            };

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.InitiateOnlinePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("Amount must be greater than 0");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_AmountNegative_ReturnsBadRequest([Greedy] OnlinePaymentsController controller)
        {
            // Arrange
            var request = new OnlinePaymentRequestDto
            {
                UserId = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                Regulator = "Test Regulator",
                Reference = "Test Reference",
                Amount = -1, // Invalid amount
                Description = PaymentDescConstants.RegistrationFee
            };

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.InitiateOnlinePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("Amount must be greater than 0");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_NextUrlIsNull_ReturnsErrorUrl(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OnlinePaymentsController>> loggerMock,
            [Greedy] OnlinePaymentsController controller,
            [Frozen] OnlinePaymentRequestDto request)
        { 
            // Arrange
            var cancellationToken = new CancellationToken();
            OnlinePaymentResponseDto expectedResponse = new OnlinePaymentResponseDto { NextUrl = null };
            onlinePaymentsServiceMock.Setup(s => s.InitiateOnlinePaymentAsync(request, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.InitiateOnlinePayment(request, cancellationToken);

            // Assert

            using (new AssertionScope())
            {
                loggerMock.Verify(
                    x => x.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(LogMessages.NextUrlNull)),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                    Times.Once);

                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain("window.location.href = 'https://example.com/error'");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_InvalidRequest_ReturnsBadRequest(
            [Greedy] OnlinePaymentsController controller)
        {
            // Arrange
            var request = new OnlinePaymentRequestDto { Description = PaymentDescConstants.RegistrationFee }; // Invalid request
            controller.ModelState.AddModelError("Amount", "Amount is required");

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.InitiateOnlinePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<SerializableError>();
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_ThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Greedy] OnlinePaymentsController controller)
        {
            // Arrange
            var invalidRequest = new OnlinePaymentRequestDto { Description = PaymentDescConstants.RegistrationFee };
            var validationException = new ValidationException("Validation error");
            var cancellationToken = new CancellationToken();

            onlinePaymentsServiceMock.Setup(s => s.InitiateOnlinePaymentAsync(invalidRequest, cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await controller.InitiateOnlinePayment(invalidRequest, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlinePayment_ThrowsException_ReturnsErrorUrl(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Greedy] OnlinePaymentsController controller,
            [Frozen] OnlinePaymentRequestDto request)
        {
            // Arrange
            var exception = new Exception("Some error");
            var cancellationToken = new CancellationToken();
            var errorUrl = "https://example.com/error";

            onlinePaymentsServiceMock.Setup(s => s.InitiateOnlinePaymentAsync(request, cancellationToken)).ThrowsAsync(exception);

            // Act
            var result = await controller.InitiateOnlinePayment(request, cancellationToken);

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
        public async Task CompleteOnlinePayment_ValidExternalPaymentId_ReturnsOk(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Greedy] OnlinePaymentsController controller,
            [Frozen] Guid externalPaymentId,
            [Frozen] CompleteOnlinePaymentResponseDto expectedResponse)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            onlinePaymentsServiceMock.Setup(s => s.CompleteOnlinePaymentAsync(externalPaymentId, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.CompleteOnlinePayment(externalPaymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
                onlinePaymentsServiceMock.Verify(s => s.CompleteOnlinePaymentAsync(externalPaymentId, cancellationToken), Times.Once);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_EmptyExternalPaymentId_ReturnsBadRequest(
            [Greedy] OnlinePaymentsController controller)
        {
            // Arrange
            var externalPaymentId = Guid.Empty;
            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.CompleteOnlinePayment(externalPaymentId, cancellationToken);

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
        public async Task CompleteOnlinePayment_ThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Greedy] OnlinePaymentsController controller,
            [Frozen] Guid externalPaymentId)
        {
            // Arrange
            var validationException = new ValidationException("Validation error");
            var cancellationToken = new CancellationToken();

            onlinePaymentsServiceMock.Setup(s => s.CompleteOnlinePaymentAsync(externalPaymentId, cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await controller.CompleteOnlinePayment(externalPaymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CompleteOnlinePayment_ThrowsException_ReturnsErrorUrl(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Greedy] OnlinePaymentsController controller,
            [Frozen] Guid externalPaymentId)
        {
            // Arrange
            var exception = new Exception("Some error");
            var cancellationToken = new CancellationToken();
            var errorUrl = "https://example.com/error";

            onlinePaymentsServiceMock.Setup(s => s.CompleteOnlinePaymentAsync(externalPaymentId, cancellationToken)).ThrowsAsync(exception);

            // Act
            var result = await controller.CompleteOnlinePayment(externalPaymentId, cancellationToken);

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

        //V2
        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePayment_ValidRequest_ReturnsRedirectResponse(
           [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
           [Greedy] OnlinePaymentsController controller,
           [Frozen] OnlinePaymentRequestV2Dto request,
           [Frozen] OnlinePaymentResponseDto expectedResponse)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            onlinePaymentsServiceMock.Setup(s => s.InitiateOnlinePaymentV2Async(request, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            IActionResult result = await controller.InitiateOnlinePaymentV2(request, cancellationToken);

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
        public async Task InitiateV2OnlinePayment_ThrowsException_ReturnsErrorUrl(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Greedy] OnlinePaymentsController controller,
            [Frozen] OnlinePaymentRequestV2Dto request)
        {
            // Arrange
            var exception = new Exception("Some error");
            var cancellationToken = new CancellationToken();
            var errorUrl = "https://example.com/error";

            onlinePaymentsServiceMock.Setup(s => s.InitiateOnlinePaymentV2Async(request, cancellationToken)).ThrowsAsync(exception);

            // Act
            IActionResult result = await controller.InitiateOnlinePaymentV2(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain($"window.location.href = '{errorUrl}'");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateOnlineV2Payment_NextUrlIsNull_ReturnsErrorUrl(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OnlinePaymentsController>> loggerMock,
            [Greedy] OnlinePaymentsController controller,
            [Frozen] OnlinePaymentRequestV2Dto request)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            OnlinePaymentResponseDto expectedResponse = new OnlinePaymentResponseDto { NextUrl = null };
            onlinePaymentsServiceMock.Setup(s => s.InitiateOnlinePaymentV2Async(request, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            IActionResult result = await controller.InitiateOnlinePaymentV2(request, cancellationToken);

            // Assert

            using (new AssertionScope())
            {
                loggerMock.Verify(
                    x => x.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(LogMessages.NextUrlNull)),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                    Times.Once);

                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain("window.location.href = 'https://example.com/error'");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePayment_InvalidRequest_ReturnsBadRequest(
            [Greedy] OnlinePaymentsController controller)
        {
            // Arrange
            var request = new OnlinePaymentRequestV2Dto { Description = PaymentDescConstants.RegistrationFee }; // Invalid request
            controller.ModelState.AddModelError("Amount", "Amount is required");

            var cancellationToken = new CancellationToken();

            // Act
            IActionResult result = await controller.InitiateOnlinePaymentV2(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<SerializableError>();
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePayment_ThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IOnlinePaymentsService> onlinePaymentsServiceMock,
            [Greedy] OnlinePaymentsController controller)
        {
            // Arrange
            var invalidRequest = new OnlinePaymentRequestV2Dto { Description = PaymentDescConstants.RegistrationFee };
            var validationException = new ValidationException("Validation error");
            var cancellationToken = new CancellationToken();

            onlinePaymentsServiceMock.Setup(s => s.InitiateOnlinePaymentV2Async(invalidRequest, cancellationToken)).ThrowsAsync(validationException);

            // Act
            IActionResult result = await controller.InitiateOnlinePaymentV2(invalidRequest, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiateV2OnlinePayment_AmountNegative_ReturnsBadRequest([Greedy] OnlinePaymentsController controller)
        {
            // Arrange
            var request = new OnlinePaymentRequestV2Dto
            {
                UserId = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                Regulator = "Test Regulator",
                Reference = "Test Reference",
                Amount = -1, // Invalid amount
                Description = PaymentDescConstants.RegistrationFee
            };

            var cancellationToken = new CancellationToken();

            // Act
            IActionResult result = await controller.InitiateOnlinePaymentV2(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("Amount must be greater than 0");
            }
        }
    }
}