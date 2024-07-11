using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace EPR.Payment.Facade.UnitTests.RESTServices
{
    [TestClass]
    public class HttpPaymentsServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptions<Service>> _configMock = null!;
        private HttpPaymentsService _httpPaymentsService = null!;
        private InsertPaymentRequestDto _insertPaymentRequestDto = null!;
        private UpdatePaymentRequestDto _updatePaymentRequestDto = null!;
        private Guid _paymentId;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "payments",
                HttpClientName = "HttpClientName"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _insertPaymentRequestDto = new InsertPaymentRequestDto
            {
                UserId = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                Reference = "123456",
                Regulator = "Regulator",
                Amount = 100,
                ReasonForPayment = "Payment description",
                Status = Common.Enums.PaymentStatus.Initiated
            };
            _updatePaymentRequestDto = new UpdatePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                GovPayPaymentId = "govpay123",
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid(),
                Reference = "123456",
                Status = Common.Enums.PaymentStatus.InProgress
            };
            _paymentId = Guid.NewGuid();

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new Mock<HttpMessageHandler>().Object));

            _httpPaymentsService = new HttpPaymentsService(
                _httpContextAccessorMock.Object,
                httpClientFactoryMock.Object, // Pass the mocked HttpClientFactory
                _configMock.Object);
        }

        [TestMethod]
        public async Task InsertPaymentAsync_Success_ReturnsGuid()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_paymentId), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);

            var httpPaymentsService = new HttpPaymentsService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient), // Pass the mocked HttpClientFactory
                _configMock!.Object);

            var cancellationToken = new CancellationToken();

            // Act
            var result = await httpPaymentsService.InsertPaymentAsync(_insertPaymentRequestDto, cancellationToken);

            // Assert
            result.Should().Be(_paymentId);
        }

        [TestMethod]
        public async Task InsertPaymentAsync_Failure_ThrowsException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Error occurred while inserting payment status."));

            var httpClient = new HttpClient(handlerMock.Object);

            var httpPaymentsService = new HttpPaymentsService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient), // Pass the mocked HttpClientFactory
                _configMock!.Object);

            var cancellationToken = new CancellationToken();

            // Act & Assert
            Func<Task> act = async () => await httpPaymentsService.InsertPaymentAsync(_insertPaymentRequestDto, cancellationToken);
            await act.Should().ThrowAsync<Exception>().WithMessage("Error occurred while inserting payment status.");
        }

        [TestMethod]
        public async Task UpdatePaymentAsync_Success_UpdatesPaymentStatus()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK
                       });

            var httpClient = new HttpClient(handlerMock.Object);

            var httpPaymentsService = new HttpPaymentsService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient), // Pass the mocked HttpClientFactory
                _configMock!.Object);

            var cancellationToken = new CancellationToken();

            // Act
            await httpPaymentsService.UpdatePaymentAsync(_paymentId, _updatePaymentRequestDto, cancellationToken);

            // Assert
            // No exception means success
        }

        [TestMethod]
        public async Task UpdatePayment_Failure_ThrowsException()
        {
            // Arrange
            var updatePaymentRequestDto = _updatePaymentRequestDto!;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Error occurred while updating payment status."));

            var httpClient = new HttpClient(handlerMock.Object);

            var httpPaymentsService = new HttpPaymentsService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient), // Pass the mocked HttpClientFactory
                _configMock!.Object);

            var cancellationToken = new CancellationToken();

            // Act & Assert
            Func<Task> act = async () => await httpPaymentsService.UpdatePaymentAsync(updatePaymentRequestDto.Id, updatePaymentRequestDto, cancellationToken);
            await act.Should().ThrowAsync<Exception>().WithMessage("Error occurred while updating payment status.");
        }
    }

    public class HttpClientFactoryMock : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public HttpClientFactoryMock(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }
}
