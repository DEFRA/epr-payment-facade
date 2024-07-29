using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Internal.Payments;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using EPR.Payment.Facade.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPR.Payment.Facade.UnitTests.Controllers
{
    [TestClass]
    public class PaymentsControllerTests
    {
        private IFixture? fixture;

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
        public async Task InitiatePayment_SetsPaymentDataCookie(
    [Frozen] Mock<IPaymentsService> paymentsServiceMock,
    [Frozen] Mock<ILogger<PaymentsController>> loggerMock,
    [Frozen] Mock<ICookieService> cookieServiceMock,
    PaymentsController controller,
    PaymentRequestDto request,
    PaymentResponseDto expectedResponse)
        {
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "govPayPaymentId";

            var paymentData = new PaymentCookieDataDto
            {
                ExternalPaymentId = externalPaymentId,
                UpdatedByUserId = request.UserId!.Value,
                UpdatedByOrganisationId = request.OrganisationId!.Value,
                GovPayPaymentId = govPayPaymentId
            };

            var expectedEncryptedPaymentData = "mock-encrypted-data";
            var base64EncodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedEncryptedPaymentData));
            var urlEncodedData = Uri.EscapeDataString(base64EncodedData);

            cookieServiceMock.Setup(cs => cs.SetPaymentDataCookie(It.IsAny<HttpResponse>(), It.IsAny<PaymentCookieDataDto>()))
                             .Callback<HttpResponse, PaymentCookieDataDto>((response, data) =>
                             {
                                 response.Cookies.Append("PaymentData", base64EncodedData, new CookieOptions
                                 {
                                     HttpOnly = true,
                                     Secure = true,
                                     SameSite = SameSiteMode.Strict
                                 });
                             });

            expectedResponse.ExternalPaymentId = externalPaymentId;
            expectedResponse.GovPayPaymentId = govPayPaymentId;
            expectedResponse.NextUrl = "https://example.com/next";
            paymentsServiceMock.Setup(s => s.InitiatePaymentAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.InitiatePayment(request, CancellationToken.None);

            // Assert
            var cookies = controller.HttpContext.Response.Headers["Set-Cookie"].ToString();

            // Ensure that the Set-Cookie header contains the expected encrypted data
            cookies.Should().Contain("PaymentData=");
            cookies.Should().Contain(urlEncodedData);

            // Verify that SetPaymentDataCookie was called with the correct parameters
            cookieServiceMock.Verify(cs => cs.SetPaymentDataCookie(It.IsAny<HttpResponse>(),
                It.Is<PaymentCookieDataDto>(data =>
                    data.ExternalPaymentId == paymentData.ExternalPaymentId &&
                    data.UpdatedByUserId == paymentData.UpdatedByUserId &&
                    data.UpdatedByOrganisationId == paymentData.UpdatedByOrganisationId &&
                    data.GovPayPaymentId == paymentData.GovPayPaymentId)), Times.Once);

            // Verify that the PaymentResponseDto contains the correct values
            expectedResponse.ExternalPaymentId.Should().Be(externalPaymentId);
            expectedResponse.GovPayPaymentId.Should().Be(govPayPaymentId);
            expectedResponse.NextUrl.Should().Be("https://example.com/next");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_ValidRequest_ReturnsCorrectFields(
            [Frozen] Mock<IPaymentsService> paymentsServiceMock,
            PaymentsController controller,
            PaymentRequestDto request)
        {
            // Arrange
            var externalPaymentId = Guid.NewGuid();
            var govPayPaymentId = "govPayPaymentId";
            var expectedResponse = new PaymentResponseDto
            {
                ExternalPaymentId = externalPaymentId,
                GovPayPaymentId = govPayPaymentId,
                NextUrl = "https://example.com/next"
            };

            paymentsServiceMock.Setup(s => s.InitiatePaymentAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.InitiatePayment(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<RedirectResult>();
            var redirectResult = result as RedirectResult;
            redirectResult?.Url.Should().Be("https://example.com/next");

            // Verify that the PaymentResponseDto contains the correct values
            expectedResponse.ExternalPaymentId.Should().Be(externalPaymentId);
            expectedResponse.GovPayPaymentId.Should().Be(govPayPaymentId);
            expectedResponse.NextUrl.Should().Be("https://example.com/next");
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

            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

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

            var contentResult = result as ContentResult;
            contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            contentResult?.ContentType.Should().Be("text/html");
            contentResult?.Content.Should().Contain("window.location.href = 'https://example.com/error'");
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
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("Amount must be greater than 0");
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
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("Amount must be greater than 0");
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

            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.InitiatePayment(request, cancellationToken);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain($"window.location.href = '{errorUrl}'");
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
            var expectedResponse = new CompletePaymentResponseDto
            {
                Status = PaymentStatus.Success,
                Message = "Payment succeeded"
            };
            paymentsServiceMock.Setup(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.CompletePayment(govPayPaymentId, completeRequest, cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
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
            badRequestResult?.Value.Should().Be(ExceptionMessages.GovPayPaymentIdNull);
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
            badRequestResult?.Value.Should().Be(ExceptionMessages.GovPayPaymentIdNull);
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

            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.CompletePayment(govPayPaymentId, completeRequest, cancellationToken);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().BeOfType<ContentResult>();
                var contentResult = result as ContentResult;
                contentResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                contentResult?.ContentType.Should().Be("text/html");
                contentResult?.Content.Should().Contain($"window.location.href = '{errorUrl}'");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_MissingExternalPaymentId_ReturnsBadRequest(
            PaymentsController controller)
        {
            // Arrange
            var govPayPaymentId = "validId";
            var request = new CompletePaymentRequestDto
            {
                // Missing ExternalPaymentId
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };
            controller.ModelState.AddModelError("ExternalPaymentId", "ExternalPaymentId is required");

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.CompletePayment(govPayPaymentId, request, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_MissingUpdatedByUserId_ReturnsBadRequest(
            PaymentsController controller)
        {
            // Arrange
            var govPayPaymentId = "validId";
            var request = new CompletePaymentRequestDto
            {
                ExternalPaymentId = Guid.NewGuid(),
                // Missing UpdatedByUserId
                UpdatedByOrganisationId = Guid.NewGuid()
            };
            controller.ModelState.AddModelError("UpdatedByUserId", "UpdatedByUserId is required");

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.CompletePayment(govPayPaymentId, request, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod, AutoMoqData]
        public async Task CompletePayment_MissingUpdatedByOrganisationId_ReturnsBadRequest(
            PaymentsController controller)
        {
            // Arrange
            var govPayPaymentId = "validId";
            var request = new CompletePaymentRequestDto
            {
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                // Missing UpdatedByOrganisationId
            };
            controller.ModelState.AddModelError("UpdatedByOrganisationId", "UpdatedByOrganisationId is required");

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.CompletePayment(govPayPaymentId, request, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
