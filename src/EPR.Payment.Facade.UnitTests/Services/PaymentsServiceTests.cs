using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.Dtos.Response.Common;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

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
            response.Should().BeEquivalentTo(expectedResponse);
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
            await service.Invoking(async x => await x.InitiatePayment(request))
                .Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task InitiatePayment_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);

            // Act & Assert
            await service.Invoking(async x => await x.InitiatePayment(null))
                .Should().ThrowAsync<ArgumentNullException>();
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
            response.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task GetPaymentStatus_NullPaymentId_ThrowsArgumentException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);

            // Act & Assert
            await service.Invoking(async x => await x.GetPaymentStatus(null))
                .Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task GetPaymentStatus_EmptyPaymentId_ThrowsArgumentException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);

            // Act & Assert
            await service.Invoking(async x => await x.GetPaymentStatus(""))
                .Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task InsertPaymentStatus_Success()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);
            var status = new PaymentStatusInsertRequestDto { Status = "Inserted" };
            var paymentId = "123";

            // Act
            Func<Task> action = async () => await service.InsertPaymentStatus(paymentId, status);

            // Assert
            await action.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task InsertPaymentStatus_NullOrEmptyPaymentId_ThrowsArgumentException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);
            var status = new PaymentStatusInsertRequestDto { Status = "Inserted" };

            // Act & Assert
            await service.Invoking(async x => await x.InsertPaymentStatus(null, status))
                .Should().ThrowAsync<ArgumentException>();

            await service.Invoking(async x => await x.InsertPaymentStatus("", status))
                .Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task InsertPaymentStatus_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new PaymentsService(_httpGovPayServiceMock.Object, _httpPaymentServiceMock.Object);

            // Act & Assert
            await service.Invoking(async x => await x.InsertPaymentStatus("123", null))
                .Should().ThrowAsync<ArgumentNullException>();
        }
    }
}
