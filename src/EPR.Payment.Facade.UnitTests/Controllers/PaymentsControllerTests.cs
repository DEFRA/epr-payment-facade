using EPR.Payment.Facade.Controllers;
using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using EPR.Payment.Facade.Common.Dtos.Response.Common;

namespace EPR.Payment.Facade.Tests
{
    [TestClass]
    public class PaymentsControllerTests
    {
        private PaymentsController? _controller;
        private Mock<IPaymentsService>? _paymentServiceMock;
        private Mock<ILogger<PaymentsController>>? _loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            _paymentServiceMock = new Mock<IPaymentsService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();

            _controller = new PaymentsController(_paymentServiceMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task InitiatePayment_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new PaymentRequestDto { Amount = 14500, Reference = "12345", Description = "Pay your council tax", return_url = "https://your.service.gov.uk/completed" };
            var expectedResponse = new PaymentResponseDto { Amount = 14500, Reference = "12345", Description = "Pay your council tax", PaymentId = "no7kr7it1vjbsvb7r402qqrv86", Email = "sherlock.holmes@example.com" };
            _paymentServiceMock.Setup(service => service.InitiatePayment(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.InitiatePayment(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>().Which.ActionName.Should().Be(nameof(_controller.GetPaymentStatus));
            result.Result.Should().BeOfType<CreatedAtActionResult>().Which.RouteValues.Should().ContainKey("paymentId").WhoseValue.Should().Be(expectedResponse.PaymentId);
            result.Result.Should().BeOfType<CreatedAtActionResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task InitiatePayment_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new PaymentRequestDto { Amount = 14500, Reference = "12345", Description = "Pay your council tax", return_url = "https://your.service.gov.uk/completed" };
            _paymentServiceMock.Setup(service => service.InitiatePayment(request)).ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.InitiatePayment(request);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod]
        public async Task InitiatePayment_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange: Create a request with missing required fields
            var request = new PaymentRequestDto(); // Invalid request with missing required fields

            // Add error messages to ModelState to simulate missing required fields
            _controller.ModelState.AddModelError(nameof(PaymentRequestDto.Amount), "The Amount field is required.");
            _controller.ModelState.AddModelError(nameof(PaymentRequestDto.Reference), "The Reference field is required.");
            _controller.ModelState.AddModelError(nameof(PaymentRequestDto.Description), "The Description field is required.");
            _controller.ModelState.AddModelError(nameof(PaymentRequestDto.return_url), "The Return URL field is required.");

            // Act: Call the action method
            var result = await _controller.InitiatePayment(request);

            // Assert: Check if the response is BadRequest and has the correct status code
            result.Result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [TestMethod]
        public async Task GetPaymentStatus_ValidRequest_ReturnsOk()
        {
            // Arrange
            var paymentId = "12345";
            var expectedResponse = SetupPaymentStatusResponseDto();

            _paymentServiceMock.Setup(service => service.GetPaymentStatus(paymentId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPaymentStatus(paymentId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task GetPaymentStatus_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var paymentId = "12345";
            _paymentServiceMock.Setup(service => service.GetPaymentStatus(paymentId)).ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.GetPaymentStatus(paymentId);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod]
        public async Task GetPaymentStatus_PaymentNotFound_ReturnsNotFound()
        {
            // Arrange
            var paymentId = "invalidId";
            _paymentServiceMock.Setup(service => service.GetPaymentStatus(paymentId)).ReturnsAsync((PaymentStatusResponseDto)null);

            // Act
            var result = await _controller.GetPaymentStatus(paymentId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task GetPaymentStatus_EmptyPaymentId_ReturnsBadRequest()
        {
            // Arrange: Set up the scenario with an empty paymentId
            string emptyPaymentId = string.Empty;

            // Act: Call the action method with the empty paymentId
            var result = await _controller.GetPaymentStatus(emptyPaymentId);

            // Assert: Check if the response is BadRequest and has the correct status code
            result.Result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [TestMethod]
        public async Task InsertPaymentStatus_ValidRequest_ReturnsOk()
        {
            // Arrange
            var paymentId = "12345";
            var request = new PaymentStatusInsertRequestDto { Status = "Inserted" };

            // Act
            var result = await _controller.InsertPaymentStatus(paymentId, request);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [TestMethod]
        public async Task InsertPaymentStatus_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var paymentId = "12345";
            var request = new PaymentStatusInsertRequestDto { };

            // Setup the mock to expect a call to InsertPaymentStatus with paymentId and request
            _paymentServiceMock.Setup(service => service.InsertPaymentStatus(paymentId, request))
                               .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.InsertPaymentStatus(paymentId, request);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod]
        public async Task InsertPaymentStatus_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var paymentId = "12345";
            //TODO : PS - need to setup tests for exact model for invalid state
            var request = new PaymentStatusInsertRequestDto { /* Invalid request data */ };
            _controller.ModelState.AddModelError("PropertyName", "Error message"); // Add a model state error

            // Act
            var result = await _controller.InsertPaymentStatus(paymentId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        private PaymentStatusResponseDto SetupPaymentStatusResponseDto()
        {
            return new PaymentStatusResponseDto
            {
                Amount = 14500,
                Reference = "12345",
                Description = "Pay your council tax",
                PaymentId = "no7kr7it1vjbsvb7r402qqrv86",
                Email = "sherlock.holmes@example.com",
                State = new State { Status = "Success", Finished = true },
                Metadata = new Metadata { LedgerCode = "1234", InternalReferenceNumber = 5678 },
                RefundSummary = new RefundSummary { Status = "Refunded", AmountAvailable = 1000, AmountSubmitted = 500 },
                SettlementSummary = new SettlementSummary(), // Ensure correct namespace here
                CardDetails = new CardDetails
                {
                    LastDigitsCardNumber = "1234",
                    FirstDigitsCardNumber = "456",
                    CardholderName = "John Doe",
                    ExpiryDate = "12/23",
                    BillingAddress = new BillingAddress { Line1 = "123 Street", City = "City", Postcode = "12345", Country = "Country" },
                    CardBrand = "Visa",
                    CardType = "Debit",
                    WalletType = "Apple Pay"
                },
                DelayedCapture = true,
                Moto = false,
                ReturnUrl = "https://your.service.gov.uk/completed",
                AuthorisationMode = "3D Secure",
                Links = new Links
                {
                    Self = new Self { Href = "https://example.com/self", Method = "GET" },
                    NextUrl = new NextUrl { Href = "https://example.com/next", Method = "POST" },
                    NextUrlPost = new NextUrlPost { Href = "https://example.com/nextpost", Method = "POST" },
                    Events = new Events { Href = "https://example.com/events", Method = "GET" },
                    Refunds = new Refunds { Href = "https://example.com/refunds", Method = "POST" },
                    Cancel = new Cancel { Href = "https://example.com/cancel", Method = "DELETE" }
                }
            };
        }


    }
}
