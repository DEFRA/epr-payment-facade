using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.Common;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services
{
    [TestClass]
    public class PaymentsServiceTests
    {
        private Mock<IHttpGovPayService> _httpGovPayServiceMock;
        private Mock<IHttpPaymentsService> _httpPaymentsServiceMock;
        private Mock<ILogger<PaymentsService>> _loggerMock;
        private PaymentsService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _httpGovPayServiceMock = new Mock<IHttpGovPayService>();
            _httpPaymentsServiceMock = new Mock<IHttpPaymentsService>();
            _loggerMock = new Mock<ILogger<PaymentsService>>();
            _service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object);
        }

        [TestMethod]
        public async Task InitiatePayment_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Amount = 100,
                ReferenceNumber = "REF123",
                ReasonForPayment = "Test Payment",
                return_url = "https://example.com/callback",
                OrganisationId = "Org123",
                UserId = "User123",
                Regulator = "Reg123"
            };
            var expectedResponse = new PaymentResponseDto
            {
                PaymentId = "12345",
                ReturnUrl = "https://example.com/response"
            };

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(request)).ReturnsAsync(expectedResponse);
            _httpPaymentsServiceMock.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>())).ReturnsAsync(Guid.NewGuid);

            // Act
            var response = await _service.InitiatePaymentAsync(request);

            // Assert
            response.Should().BeEquivalentTo(expectedResponse);
            _httpPaymentsServiceMock.Verify(s => s.UpdatePaymentAsync(
                It.IsAny<Guid>(),
                It.Is<UpdatePaymentRequestDto>(r => r.Status == PaymentStatus.InProgress && r.GovPayPaymentId == "12345")), Times.Once);
        }
        [TestMethod]
        public async Task InitiatePayment_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _service.Invoking(async s => await s.InitiatePaymentAsync(null))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [DataTestMethod]
        [DataRow(null, "REF123", "Test Payment", "https://example.com/callback", "Org123", "User123", "Reg123", "Amount is required")]
        [DataRow(100, null, "Test Payment", "https://example.com/callback", "Org123", "User123", "Reg123", "Reference Number is required")]
        [DataRow(100, "REF123", null, "https://example.com/callback", "Org123", "User123", "Reg123", "Reason For Payment is required")]
        [DataRow(100, "REF123", "Test Payment", null, "Org123", "User123", "Reg123", "Return URL is required")]
        [DataRow(100, "REF123", "Test Payment", "https://example.com/callback", null, "User123", "Reg123", "Organisation ID is required")]
        [DataRow(100, "REF123", "Test Payment", "https://example.com/callback", "Org123", null, "Reg123", "User ID is required")]
        [DataRow(100, "REF123", "Test Payment", "https://example.com/callback", "Org123", "User123", null, "Regulator is required")]
        public async Task InitiatePayment_MissingFields_ThrowsValidationException(
            int? amount, string referenceNumber, string reasonForPayment, string returnUrl,
            string organisationId, string userId, string regulator, string expectedMessage)
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Amount = amount,
                ReferenceNumber = referenceNumber,
                ReasonForPayment = reasonForPayment,
                return_url = returnUrl,
                OrganisationId = organisationId,
                UserId = userId,
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
                ReferenceNumber = "REF123",
                ReasonForPayment = "Test Payment",
                return_url = "https://example.com/callback",
                OrganisationId = "Org123",
                UserId = "User123",
                Regulator = "Reg123"
            };
            var expectedResponse = new PaymentResponseDto
            {
                PaymentId = "12345",
                ReturnUrl = "https://example.com/response"
            };

            _httpGovPayServiceMock.Setup(s => s.InitiatePaymentAsync(request)).ReturnsAsync(expectedResponse);
            _httpPaymentsServiceMock
                .Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            await _service.Invoking(async s => await s.InitiatePaymentAsync(request))
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
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = "User123",
                UpdatedByOrganisationId = "Org123"
            };

            // Act
            await _service.CompletePaymentAsync(govPayPaymentId, completeRequest);

            // Assert
            _httpPaymentsServiceMock.Verify(s => s.UpdatePaymentAsync(
                completeRequest.ExternalPaymentId,
                It.Is<UpdatePaymentRequestDto>(r => r.Status == PaymentStatus.Success && r.GovPayPaymentId == govPayPaymentId)), Times.Once);
        }

        [TestMethod]
        public async Task CompletePayment_NullGovPayPaymentId_ThrowsArgumentException()
        {
            var completeRequest = new CompletePaymentRequestDto
            {
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = "User123",
                UpdatedByOrganisationId = "Org123"
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
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = "User123",
                UpdatedByOrganisationId = "Org123"
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
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = "User123",
                UpdatedByOrganisationId = "Org123"
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
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = "User123",
                UpdatedByOrganisationId = "Org123"
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
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = "User123",
                UpdatedByOrganisationId = "Org123"
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
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = "User123",
                UpdatedByOrganisationId = "Org123"
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
                ExternalPaymentId = Guid.NewGuid(),
                UpdatedByUserId = "User123",
                UpdatedByOrganisationId = "Org123"
            };

            // Act & Assert
            await _service.Invoking(async s => await s.CompletePaymentAsync(govPayPaymentId, completeRequest))
                .Should().ThrowAsync<Exception>().WithMessage("An unexpected error occurred while updating the payment status.");
        }
    }
}