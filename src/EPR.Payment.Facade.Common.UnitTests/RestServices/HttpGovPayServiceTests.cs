using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Payment.Facade.UnitTests.RESTServices
{
    [TestClass]
    public class HttpGovPayServiceTests
    {
        private Mock<IHttpContextAccessor>? _httpContextAccessorMock;
        private Mock<IOptions<Service>>? _configMock;
        private HttpGovPayService? _httpGovPayService;
        private GovPayResponseDto? _expectedResponse;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "payments",
                BearerToken = "dummyBearerToken"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _expectedResponse = new GovPayResponseDto
            {
                PaymentId = "12345",
                Amount = 100,
                Reference = "123456",
                Description = "Payment description",
                ReturnUrl = "https://example.com/return",
                State = new StateDto { Status = "created", Finished = false }
            };

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new Mock<HttpMessageHandler>().Object));

            _httpGovPayService = new HttpGovPayService(
                _httpContextAccessorMock.Object,
                httpClientFactoryMock.Object,
                _configMock.Object);
        }

        [TestMethod]
        public async Task InitiatePayment_Success_ReturnsPaymentResponseDto()
        {
            // Arrange
            var paymentRequestDto = new GovPayPaymentRequestDto
            {
                Amount = 14500,
                Reference = "12345",
                Description = "Pay your council tax",
                return_url = "https://your.service.gov.uk/completed",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "regulator"
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_expectedResponse)),
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            var httpGovPayService = new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);

            var cancellationToken = new CancellationToken();

            // Act
            var result = await httpGovPayService.InitiatePaymentAsync(paymentRequestDto, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.PaymentId.Should().Be(_expectedResponse!.PaymentId);
            result.Amount.Should().Be(_expectedResponse.Amount);
            result.Reference.Should().Be(_expectedResponse.Reference);
            result.Description.Should().Be(_expectedResponse.Description);
            result.ReturnUrl.Should().Be(_expectedResponse.ReturnUrl);
        }

        [TestMethod]
        public async Task InitiatePayment_Failure_ThrowsException()
        {
            // Arrange
            var paymentRequestDto = new GovPayPaymentRequestDto
            {
                Amount = 14500,
                Reference = "12345",
                Description = "Pay your council tax",
                return_url = "https://your.service.gov.uk/completed",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "regulator"
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Error occurred while initiating payment."));

            var httpClient = new HttpClient(handlerMock.Object);
            var httpGovPayService = new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);

            var cancellationToken = new CancellationToken();

            // Act & Assert
            Func<Task> act = async () => await httpGovPayService.InitiatePaymentAsync(paymentRequestDto, cancellationToken);
            await act.Should().ThrowAsync<Exception>().WithMessage("Error occurred while initiating payment.");
        }

        [TestMethod]
        public async Task GetPaymentStatus_Success_ReturnsPaymentStatusResponseDto()
        {
            // Arrange
            var paymentId = "12345";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(new PaymentStatusResponseDto())),
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            var httpGovPayService = new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);

            var cancellationToken = new CancellationToken();

            // Act
            var result = await httpGovPayService.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetPaymentStatus_Failure_ThrowsException()
        {
            // Arrange
            var paymentId = "123";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Error occurred while retrieving payment status."));

            var httpClient = new HttpClient(handlerMock.Object);
            var httpGovPayService = new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);

            var cancellationToken = new CancellationToken();

            // Act & Assert
            Func<Task> act = async () => await httpGovPayService.GetPaymentStatusAsync(paymentId, cancellationToken);
            await act.Should().ThrowAsync<Exception>().WithMessage("Error occurred while retrieving payment status.");
        }
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
