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
        public async Task InitiatePayment_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new PaymentRequestDto { Amount = 14500, Reference = "12345", Description = "Pay your council tax", return_url = "https://your.service.gov.uk/completed" };
            var expectedResponse = new PaymentResponseDto { Amount = 14500, Reference = "12345", Description = "Pay your council tax", PaymentId = "no7kr7it1vjbsvb7r402qqrv86", Email = "sherlock.holmes@example.com" };
            _paymentServiceMock.Setup(service => service.InitiatePayment(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.InitiatePayment(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.AreEqual(expectedResponse, okResult.Value);
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
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult)); 
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode); 
        }


        [TestMethod]
        public async Task GetPaymentStatus_ValidRequest_ReturnsOk()
        {
            // Arrange
            var paymentId = "12345";
            var expectedResponse = new PaymentStatusResponseDto { Amount = 14500, Reference = "12345", Description = "Pay your council tax", PaymentId = "no7kr7it1vjbsvb7r402qqrv86", Email = "sherlock.holmes@example.com" };
            _paymentServiceMock.Setup(service => service.GetPaymentStatus(paymentId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPaymentStatus(paymentId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.AreEqual(expectedResponse, okResult.Value);
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
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult)); 
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
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
            Assert.IsInstanceOfType(result, typeof(OkResult));
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
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
