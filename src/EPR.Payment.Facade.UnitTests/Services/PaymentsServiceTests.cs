using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.Dtos.Response.Common;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.UnitTests.Services
{
    [TestClass]
    public class PaymentsServiceTests
    {
        private readonly Mock<IHttpGovPayService> _httpGovPayServiceMock = new Mock<IHttpGovPayService>();
        private readonly Mock<IHttpPaymentsService> _httpPaymentServiceMock = new Mock<IHttpPaymentsService>();

        [TestMethod]
        public async Task InitiatePayment_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);
            var request = new PaymentRequestDto
            {
                Amount = 100,
                Reference = "REF123",
                Description = "Test Payment",
                return_url = "https://example.com/callback"
            };
            var expectedResponse = new PaymentResponseDto
            {
                PaymentId = "12345",
                Amount = request.Amount,
                Reference = request.Reference,
                Description = request.Description,
                Email = "test@example.com"
            };
            _httpGovPayServiceMock.Setup(s => s.InitiatePayment(request)).ReturnsAsync(expectedResponse);

            // Act
            var response = await service.InitiatePayment(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public async Task InitiatePayment_MissingReference_ThrowsArgumentException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);
            var request = new PaymentRequestDto
            {
                Amount = 100,
                Description = "Test Payment",
                return_url = "https://example.com/callback"
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.InitiatePayment(request));
        }

        [TestMethod]
        public async Task InitiatePayment_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => service.InitiatePayment(null));
        }

        [TestMethod]
        public async Task GetPaymentStatus_ValidPaymentId_ReturnsResponse()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);
            var paymentId = "123456";
            var expectedResponse = new PaymentStatusResponseDto
            {
                PaymentId = paymentId,
                State = new State { Finished = true },
                Amount = 100,
                Description = "Test Payment"
            };
            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatus(paymentId)).ReturnsAsync(expectedResponse);

            // Act
            var response = await service.GetPaymentStatus(paymentId);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public async Task GetPaymentStatus_NullPaymentId_ThrowsArgumentException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.GetPaymentStatus(null));
        }

        [TestMethod]
        public async Task GetPaymentStatus_EmptyPaymentId_ThrowsArgumentException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.GetPaymentStatus(""));
        }

        [TestMethod]
        public async Task InsertPaymentStatus_Success()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);
            var status = new PaymentStatusInsertRequestDto { Status = "Inserted" };
            var paymentId = "123";

            // Act
            await service.InsertPaymentStatus(paymentId, status);

            // Assert
            // TODO: Add suitable asserts here when known
        }   

        [TestMethod]
        public async Task InsertPaymentStatus_NullOrEmptyPaymentId_ThrowsArgumentException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);
            var status = new PaymentStatusInsertRequestDto { Status = "Inserted" };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.InsertPaymentStatus(null, status));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.InsertPaymentStatus("", status));
        }

        [TestMethod]
        public async Task InsertPaymentStatus_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => service.InsertPaymentStatus("123", null));
        }
    }
}
