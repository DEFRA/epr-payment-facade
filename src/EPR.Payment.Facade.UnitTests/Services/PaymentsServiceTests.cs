using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.Common;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.UnitTests.Services
{
    [TestClass]
    public class PaymentsServiceTests
    {
        private Mock<IHttpGovPayService> _httpGovPayServiceMock;
        private Mock<IHttpPaymentsService> _httpPaymentsServiceMock;
        private Mock<ILogger<PaymentsService>> _loggerMock;
        private Mock<IOptions<PaymentServiceOptions>> _optionsMock;
        private PaymentsService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _httpGovPayServiceMock = new Mock<IHttpGovPayService>();
            _httpPaymentsServiceMock = new Mock<IHttpPaymentsService>();
            _loggerMock = new Mock<ILogger<PaymentsService>>();
            _optionsMock = new Mock<IOptions<PaymentServiceOptions>>();

            // Setup default values for configuration options
            _optionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            _service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                _optionsMock.Object);
        }

        [TestMethod]
        public async Task InitiatePayment_ValidRequest_ReturnsResponse()
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

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayPaymentRequestDto>())).ReturnsAsync(expectedResponse);
            _httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>())).ReturnsAsync(Guid.NewGuid());
            _httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>()));

            // Act
            var result = await _service.InitiatePaymentAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.PaymentId.Should().Be(expectedResponse.PaymentId);
            result.ReturnUrl.Should().Be(expectedResponse.ReturnUrl);
        }

        [TestMethod]
        public async Task InitiatePayment_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _service.Invoking(async s => await s.InitiatePaymentAsync(null))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [DataTestMethod]
        [DataRow(null, "REF123", "8a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d", "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d", "Reg123", "Amount is required")]
        [DataRow(100, null, "8a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d", "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d", "Reg123", "Reference is required")]
        [DataRow(100, "REF123", null, "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d", "Reg123", "Organisation ID is required")]
        [DataRow(100, "REF123", "8a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d", null, "Reg123", "User ID is required")]
        [DataRow(100, "REF123", "8a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d", "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d", null, "Regulator is required")]
        public async Task InitiatePayment_MissingFields_ThrowsValidationException(int? amount, string reference,
    string organisationId, string userId, string regulator, string expectedMessage)
        {
            // Arrange
            Guid? orgId = null;
            Guid? usrId = null;

            if (!string.IsNullOrEmpty(organisationId) && Guid.TryParse(organisationId, out var parsedOrgId))
            {
                orgId = parsedOrgId;
            }

            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsedUserId))
            {
                usrId = parsedUserId;
            }

            var request = new PaymentRequestDto
            {
                Amount = amount,
                Reference = reference,
                OrganisationId = orgId,
                UserId = usrId,
                Regulator = regulator
            };

            // Act & Assert
            var exception = await _service.Invoking(async s => await s.InitiatePaymentAsync(request))
                .Should().ThrowAsync<ValidationException>();

            exception.Which.Message.Should().Contain(expectedMessage);
        }

        [TestMethod]
        public async Task InitiatePayment_StatusUpdateValidationFails_ThrowsValidationException()
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

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayPaymentRequestDto>())).ReturnsAsync(expectedResponse);
            _httpPaymentsServiceMock.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>())).ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            var exception = await _service.Invoking(async s => await s.InitiatePaymentAsync(request))
                .Should().ThrowAsync<ValidationException>().WithMessage("Validation error");
        }

        [TestMethod]
        public async Task CompletePayment_ValidGovPayPaymentId_UpdatesPaymentStatus()
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = "success" }
            };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act
            await _service.CompletePaymentAsync(govPayPaymentId, completeRequest);

            // Assert
            _httpPaymentsServiceMock.Verify(s => s.UpdatePaymentAsync(
                completeRequest.Id,
                It.Is<UpdatePaymentRequestDto>(r => r.Status == PaymentStatus.Success && r.GovPayPaymentId == govPayPaymentId)), Times.Once);
        }

        [TestMethod]
        public async Task CompletePayment_NullGovPayPaymentId_ThrowsArgumentException()
        {
            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(null, completeRequest))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("GovPayPaymentId cannot be null or empty (Parameter 'govPayPaymentId')");
        }

        [TestMethod]
        public async Task CompletePayment_EmptyGovPayPaymentId_ThrowsArgumentException()
        {
            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(string.Empty, completeRequest))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("GovPayPaymentId cannot be null or empty (Parameter 'govPayPaymentId')");
        }

        [TestMethod]
        public async Task CompletePayment_PaymentStatusNotFound_ThrowsException()
        {
            // Arrange
            var govPayPaymentId = "12345";
            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync((PaymentStatusResponseDto)null);

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest))
                .Should().ThrowAsync<Exception>().WithMessage("Payment status not found or status is not available.");
        }

        [TestMethod]
        public async Task CompletePayment_PaymentStateNull_ThrowsException()
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = null
            };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest))
                .Should().ThrowAsync<Exception>().WithMessage("Payment status not found or status is not available.");
        }

        [TestMethod]
        public async Task CompletePayment_PaymentStatusNull_ThrowsException()
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = null }
            };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest))
                .Should().ThrowAsync<Exception>().WithMessage("Payment status not found or status is not available.");
        }

        [TestMethod]
        public async Task CompletePayment_StatusUpdateValidationFails_ThrowsValidationException()
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = "error", Code = "P0030" }
            };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);
            _httpPaymentsServiceMock
                .Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest))
                .Should().ThrowAsync<ValidationException>().WithMessage("Validation error");
        }

        [TestMethod]
        public async Task CompletePayment_StatusUpdateUnexpectedError_ThrowsException()
        {
            // Arrange
            var govPayPaymentId = "12345";
            var paymentStatusResponse = new PaymentStatusResponseDto
            {
                PaymentId = govPayPaymentId,
                State = new State { Status = "error", Code = "P0030" }
            };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);
            _httpPaymentsServiceMock
                .Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest))
                .Should().ThrowAsync<Exception>().WithMessage("An unexpected error occurred while updating the payment status.");
        }
    }
}


