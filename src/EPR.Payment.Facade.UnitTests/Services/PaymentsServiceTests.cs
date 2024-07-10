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
        private Mock<IHttpGovPayService>? _httpGovPayServiceMock;
        private Mock<IHttpPaymentsService>? _httpPaymentsServiceMock;
        private Mock<ILogger<PaymentsService>>? _loggerMock;
        private Mock<IOptions<PaymentServiceOptions>>? _optionsMock;
        private PaymentsService? _service;

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
            var expectedResponse = new GovPayResponseDto
            {
                PaymentId = "12345",
                Links = new LinksDto
                {
                    NextUrl = new LinkDto
                    {
                        Href = "https://example.com/response"
                    }
                }
            };

            _httpGovPayServiceMock?.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayPaymentRequestDto>())).ReturnsAsync(expectedResponse);
            _httpPaymentsServiceMock?.Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>())).ReturnsAsync(Guid.NewGuid());
            _httpPaymentsServiceMock?.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>()));

            // Act
            var result = await _service!.InitiatePaymentAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.NextUrl.Should().Be(expectedResponse.Links?.NextUrl?.Href);
        }

        [TestMethod]
        public async Task InitiatePayment_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _service.Invoking(async s => await s!.InitiatePaymentAsync(null))
                .Should().ThrowAsync<ArgumentNullException>();
        }
        [TestMethod]
        [DataRow(null, "REF123", "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "Reg123", "Amount is required")]
        [DataRow(100, null, "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "Reg123", "Reference is required")]
        [DataRow(100, "REF123", null, "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "Reg123", "Organisation ID is required")]
        [DataRow(100, "REF123", "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", null, "Reg123", "User ID is required")]
        [DataRow(100, "REF123", "d2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", "e2719d4e-8f4d-4e89-92c4-bb7e13db9d2b", null, "Regulator is required")]
        public async Task InitiatePayment_MissingFields_ThrowsValidationException(int? amount, string reference, string organisationId, string userId, string regulator, string expectedMessage)
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Amount = amount,
                Reference = reference,
                OrganisationId = organisationId != null ? Guid.Parse(organisationId) : (Guid?)null,
                UserId = userId != null ? Guid.Parse(userId) : (Guid?)null,
                Regulator = regulator
            };

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request))
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
            var expectedResponse = new GovPayResponseDto
            {
                PaymentId = "12345",
                Links = new LinksDto
                {
                    NextUrl = new LinkDto
                    {
                        Href = "https://example.com/response"
                    }
                }
            };

            _httpGovPayServiceMock?.Setup(s => s.InitiatePaymentAsync(It.IsAny<GovPayPaymentRequestDto>())).ReturnsAsync(expectedResponse);
            _httpPaymentsServiceMock?.Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>())).ThrowsAsync(new ValidationException("Validation error"));

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request))
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
                State = new State { Status = "success", Finished = true }
            };

            _httpGovPayServiceMock?.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act
            await _service!.CompletePaymentAsync(govPayPaymentId, completeRequest);

            // Assert
            _httpPaymentsServiceMock?.Verify(s => s.UpdatePaymentAsync(
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
            await _service.Invoking(async s => await s!.CompletePaymentAsync(null, completeRequest))
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
            await _service.Invoking(async s => await s!.CompletePaymentAsync(string.Empty, completeRequest))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("GovPayPaymentId cannot be null or empty (Parameter 'govPayPaymentId')");
        }

        [TestMethod]
        public async Task CompletePayment_PaymentStatusNotFound_ThrowsException()
        {
            // Arrange
            var govPayPaymentId = "12345";
            _httpGovPayServiceMock?.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId))
                .ReturnsAsync((PaymentStatusResponseDto?)null);

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s!.CompletePaymentAsync(govPayPaymentId, completeRequest))
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

            _httpGovPayServiceMock?.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s!.CompletePaymentAsync(govPayPaymentId, completeRequest))
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
                State = new State { Status = null, Finished = true }
            };

            _httpGovPayServiceMock?.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s!.CompletePaymentAsync(govPayPaymentId, completeRequest))
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
                State = new State { Status = "error", Finished = true }
            };

            _httpGovPayServiceMock?.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);
            _httpPaymentsServiceMock?
                .Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>()))
                .ThrowsAsync(new ValidationException("Validation error"));

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s!.CompletePaymentAsync(govPayPaymentId, completeRequest))
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
                State = new State { Status = "error", Finished = true }
            };

            _httpGovPayServiceMock?.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId)).ReturnsAsync(paymentStatusResponse);
            _httpPaymentsServiceMock?
                .Setup(s => s.UpdatePaymentAsync(It.IsAny<Guid>(), It.IsAny<UpdatePaymentRequestDto>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var completeRequest = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            // Act & Assert
            await _service.Invoking(async s => await s!.CompletePaymentAsync(govPayPaymentId, completeRequest))
                .Should().ThrowAsync<Exception>().WithMessage("An unexpected error occurred while updating the payment status.");
        }

        [TestMethod]
        public async Task InitiatePayment_ReturnUrlNotConfigured_ThrowsInvalidOperationException()
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

            // Setup the options mock to return a PaymentServiceOptions with ReturnUrl set to null
            var paymentServiceOptions = new PaymentServiceOptions { ReturnUrl = null, Description = "Payment description" };
            var optionsMock = new Mock<IOptions<PaymentServiceOptions>>();
            optionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            // Re-initialize the PaymentsService with the updated options
            var service = new PaymentsService(
                _httpGovPayServiceMock!.Object,
                _httpPaymentsServiceMock!.Object,
                _loggerMock!.Object,
                optionsMock.Object);

            // Act & Assert
            var exception = await service.Invoking(async s => await s!.InitiatePaymentAsync(request))
                .Should().ThrowAsync<InvalidOperationException>();

            exception.Which.Message.Should().Be("ReturnUrl is not configured.");
        }

        [TestMethod]
        public async Task InitiatePayment_DescriptionNotConfigured_ThrowsInvalidOperationException()
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

            // Setup the options mock to return a PaymentServiceOptions with Description set to null
            var paymentServiceOptions = new PaymentServiceOptions { ReturnUrl = "https://example.com/return", Description = null };
            var optionsMock = new Mock<IOptions<PaymentServiceOptions>>();
            optionsMock.Setup(o => o.Value).Returns(paymentServiceOptions);

            // Re-initialize the PaymentsService with the updated options
            var service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                _loggerMock.Object,
                optionsMock.Object);

            // Act & Assert
            var exception = await service.Invoking(async s => await s!.InitiatePaymentAsync(request))
                .Should().ThrowAsync<InvalidOperationException>();

            exception.Which.Message.Should().Be("Description is not configured.");
        }

        [TestMethod]
        public async Task InitiatePayment_MissingUserId_ThrowsValidationException()
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Amount = 100,
                Reference = "REF123",
                OrganisationId = Guid.NewGuid(),
                Regulator = "Reg123"
            };

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request))
                .Should().ThrowAsync<ValidationException>()
                .WithMessage("User ID is required");
        }

        [TestMethod]
        public async Task InitiatePayment_MissingOrganisationId_ThrowsValidationException()
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Amount = 100,
                Reference = "REF123",
                UserId = Guid.NewGuid(),
                Regulator = "Reg123"
            };

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request))
                .Should().ThrowAsync<ValidationException>()
                .WithMessage("Organisation ID is required");
        }


        [TestMethod]
        public async Task InitiatePayment_MissingAmount_ThrowsValidationException()
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Reference = "REF123",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "Reg123"
            };

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request))
                .Should().ThrowAsync<ValidationException>()
                .WithMessage("Amount is required");
        }


        [TestMethod]
        public async Task InitiatePayment_UserIdMissing_ThrowsValidationException()
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Amount = 100,
                Reference = "REF123",
                OrganisationId = Guid.NewGuid(),
                Regulator = "Reg123"
            };

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request))
                .Should().ThrowAsync<ValidationException>().WithMessage("User ID is required");
        }

        [TestMethod]
        public async Task InitiatePayment_OrganisationIdMissing_ThrowsValidationException()
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Amount = 100,
                Reference = "REF123",
                UserId = Guid.NewGuid(),
                Regulator = "Reg123"
            };

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request))
                .Should().ThrowAsync<ValidationException>().WithMessage("Organisation ID is required");
        }

        [TestMethod]
        public async Task InitiatePayment_AmountMissing_ThrowsValidationException()
        {
            // Arrange
            var request = new PaymentRequestDto
            {
                Reference = "REF123",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "Reg123"
            };

            // Act & Assert
            var exception = await _service.Invoking(async s => await s!.InitiatePaymentAsync(request))
                .Should().ThrowAsync<ValidationException>().WithMessage("Amount is required");
        }

        [TestMethod]
        public async Task InsertPayment_ValidationExceptionThrown_LogsAndThrows()
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

            var validationException = new ValidationException("Validation error");
            _httpPaymentsServiceMock?
                .Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>()))
                .ThrowsAsync(validationException);

            var logger = new TestLogger<PaymentsService>();
            var service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                logger,
                _optionsMock.Object);

            // Act & Assert
            var exception = await service.Invoking(async s => await s.InitiatePaymentAsync(request))
                .Should().ThrowAsync<ValidationException>().WithMessage("Validation error");

            // Verify log entry
            logger.Logs.Should().Contain(log => log.Contains("Validation error occurred while inserting payment."));
        }

        [TestMethod]
        public async Task InsertPayment_UnexpectedExceptionThrown_LogsAndThrows()
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

            var testLogger = new TestLogger<PaymentsService>();

            _httpPaymentsServiceMock
                .Setup(s => s.InsertPaymentAsync(It.IsAny<InsertPaymentRequestDto>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Create a new instance of PaymentsService with the custom test logger
            var service = new PaymentsService(
                _httpGovPayServiceMock.Object,
                _httpPaymentsServiceMock.Object,
                testLogger,
                _optionsMock.Object);

            // Act
            Func<Task> act = async () => await service.InitiatePaymentAsync(request);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("An unexpected error occurred while inserting the payment.");
            testLogger.Logs.Should().Contain("An unexpected error occurred while inserting payment.");
        }
    }
}
