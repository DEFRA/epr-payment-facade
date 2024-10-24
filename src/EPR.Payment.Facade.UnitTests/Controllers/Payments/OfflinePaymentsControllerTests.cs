using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.Common;
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
    public class OfflinePaymentsControllerTests
    {
        [TestMethod, AutoMoqData]
        public async Task InitiateOfflinePayment_ValidRequest_ReturnsNoContent(
            [Frozen] Mock<IOfflinePaymentsService> offlinePaymentsServiceMock,
            [Greedy] OfflinePaymentsController controller,
            [Frozen] OfflinePaymentRequestDto request,
            [Frozen] OfflinePaymentResponseDto expectedResponse)
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            offlinePaymentsServiceMock.Setup(s => s.OfflinePaymentAsync(request, cancellationToken)).ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.OfflinePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<NoContentResult>();
            }
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithValidArguments_ShouldInitializeCorrectly(
            [Frozen] Mock<IOfflinePaymentsService> _offlinePaymentsServiceMock,
            [Frozen] Mock<ILogger<OfflinePaymentsController>> _loggerMock,
            [Frozen] Mock<IOptions<OfflinePaymentServiceOptions>> _offlinePaymentServiceOptionsMock,
            [Frozen] OfflinePaymentServiceOptions _offlinePaymentServiceOptions)
        {
            // Arrange
            _offlinePaymentServiceOptionsMock.Setup(o => o.Value).Returns(_offlinePaymentServiceOptions);

            // Act
            var controller = new OfflinePaymentsController(
                _offlinePaymentsServiceMock.Object,
                _loggerMock.Object,
                _offlinePaymentServiceOptionsMock.Object
            );

            // Assert
            controller.Should().NotBeNull();
            controller.Should().BeAssignableTo<OfflinePaymentsController>();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullOfflinePaymentsService_ShouldThrowArgumentNullException(
            [Frozen] Mock<ILogger<OfflinePaymentsController>> _loggerMock,
            [Frozen] Mock<IOptions<OfflinePaymentServiceOptions>> _offlinePaymentServiceOptionsMock)
        {

            // Act
            Action act = () => new OfflinePaymentsController(
                null!,
                _loggerMock.Object,
                _offlinePaymentServiceOptionsMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("offlinePaymentsService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException(
            [Frozen] Mock<IOfflinePaymentsService> _offlinePaymentsServiceMock,
            [Frozen] Mock<IOptions<OfflinePaymentServiceOptions>> _offlinePaymentServiceOptionsMock)
        {

            // Act
            Action act = () => new OfflinePaymentsController(
                _offlinePaymentsServiceMock.Object,
                null!,
                _offlinePaymentServiceOptionsMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task OfflinePayment_AmountZero_ReturnsBadRequest([Greedy] OfflinePaymentsController controller)
        {
            // Arrange
            var request = new OfflinePaymentRequestDto
            {
                UserId = Guid.NewGuid(),
                Reference = "Test Reference",
                Amount = 0 // Invalid amount
            };

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.OfflinePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("Amount must be greater than 0");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task OfflinePayment_InvalidRequest_ReturnsBadRequest(
            [Greedy] OfflinePaymentsController controller)
        {
            // Arrange
            var request = new OfflinePaymentRequestDto(); // Invalid request
            controller.ModelState.AddModelError("Amount", "Amount is required");

            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.OfflinePayment(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<SerializableError>();
            }
        }

        [TestMethod, AutoMoqData]
        public async Task OfflinePayment_ThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IOfflinePaymentsService> offlinePaymentsServiceMock,
            [Greedy] OfflinePaymentsController controller)
        {
            // Arrange
            var invalidRequest = new OfflinePaymentRequestDto();
            var validationException = new ValidationException("Validation error");
            var cancellationToken = new CancellationToken();

            offlinePaymentsServiceMock.Setup(s => s.OfflinePaymentAsync(invalidRequest, cancellationToken)).ThrowsAsync(validationException);

            // Act
            var result = await controller.OfflinePayment(invalidRequest, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult?.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task OfflinePayment_ThrowsException_ReturnsErrorUrl(
            [Frozen] Mock<IOfflinePaymentsService> offlinePaymentsServiceMock,
            [Greedy] OfflinePaymentsController controller,
            [Frozen] OfflinePaymentRequestDto request)
        {
            // Arrange
            var exception = new Exception("Some error");
            var cancellationToken = new CancellationToken();
            var errorUrl = "https://example.com/error";

            offlinePaymentsServiceMock.Setup(s => s.OfflinePaymentAsync(request, cancellationToken)).ThrowsAsync(exception);

            // Act
            var result = await controller.OfflinePayment(request, cancellationToken);

            // Assert
            Assert.Fail("not sure what it should do but certainly not Error URL");

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
