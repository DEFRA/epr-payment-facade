using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
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
        private Mock<IPaymentsService> _paymentsServiceMock;
        private Mock<ILogger<PaymentsController>> _loggerMock;
        private PaymentsController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _paymentsServiceMock = new Mock<IPaymentsService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();
            _controller = new PaymentsController(_paymentsServiceMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task InitiatePayment_ValidRequest_ReturnsCreatedResponse()
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
                PaymentId = "12345",
                ReturnUrl = "https://example.com/response"
            };

            _paymentsServiceMock.Setup(s => s.InitiatePaymentAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.InitiatePayment(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            createdAtActionResult.Value.Should().BeEquivalentTo(expectedResponse);
            createdAtActionResult.ActionName.Should().Be(nameof(_controller.CompletePayment));
            createdAtActionResult.RouteValues["paymentId"].Should().Be(expectedResponse.PaymentId);

            // Verify that the ReturnUrl in the response is correctly set
            var responseDto = createdAtActionResult.Value as PaymentResponseDto;
            responseDto.ReturnUrl.Should().Be(expectedResponse.ReturnUrl);
        }

        [TestMethod]
        public async Task InitiatePayment_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaymentRequestDto(); // Invalid request
            _controller.ModelState.AddModelError("Amount", "Amount is required");

            // Act
            var result = await _controller.InitiatePayment(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
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

            // Simulate a validation exception from the service layer
            var validationException = new ValidationException("Validation error");
            _paymentsServiceMock.Setup(s => s.InitiatePaymentAsync(invalidRequest)).ThrowsAsync(validationException);

            // Act
            var result = await _controller.InitiatePayment(invalidRequest);

            // Assert
            // Check if the result is a BadRequestObjectResult
            result.Result.Should().BeOfType<BadRequestObjectResult>();

            // Check if the BadRequestObjectResult contains the expected validation error message
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
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
            _paymentsServiceMock.Setup(s => s.InitiatePaymentAsync(request)).ThrowsAsync(exception);

            // Act
            var result = await _controller.InitiatePayment(request);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            objectResult.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(exception.Message);
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

            // Act
            var result = await _controller.CompletePayment(govPayPaymentId, completeRequest);

            // Assert
            result.Should().BeOfType<OkResult>();
            _paymentsServiceMock.Verify(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest), Times.Once);
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

            // Act
            var result = await _controller.CompletePayment(null, completeRequest);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("GovPayPaymentId cannot be null or empty");
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

            // Act
            var result = await _controller.CompletePayment(string.Empty, completeRequest);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("GovPayPaymentId cannot be null or empty");
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
            _paymentsServiceMock.Setup(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest)).ThrowsAsync(validationException);

            // Act
            var result = await _controller.CompletePayment(govPayPaymentId, completeRequest);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(validationException.Message);
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
            _paymentsServiceMock.Setup(s => s.CompletePaymentAsync(govPayPaymentId, completeRequest)).ThrowsAsync(exception);

            // Act
            var result = await _controller.CompletePayment(govPayPaymentId, completeRequest);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            objectResult.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be(exception.Message);
        }
    }
}
