using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
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
        public async Task InitiatePayment_ReturnsCorrectResponse()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);
            var request = new PaymentRequestDto { Amount = 14500, Reference = "12345", Description = "Pay your council tax" };
            var expectedResponse = new PaymentResponseDto { Amount = 14500, Reference = "12345", Description = "Pay your council tax", PaymentId = "no7kr7it1vjbsvb7r402qqrv86", Email = "sherlock.holmes@example.com" };
            _httpGovPayServiceMock.Setup(s => s.InitiatePayment(request)).ReturnsAsync(expectedResponse);

            // Act
            var response = await service.InitiatePayment(request);

            // Assert
            Assert.IsNotNull(response);            
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
            // TODO - PS : add suitable asserts here when known
        }
    }
}
