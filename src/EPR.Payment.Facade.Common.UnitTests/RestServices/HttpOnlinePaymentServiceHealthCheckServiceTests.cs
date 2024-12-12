using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace EPR.Payment.Facade.Common.UnitTests.RestServices
{
    [TestClass]
    public class HttpOnlinePaymentServiceHealthCheckServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptionsMonitor<Service>> _configMonitorMock = null!;
        private Mock<HttpMessageHandler> _handlerMock = null!;

        [TestInitialize]
        public void Setup()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _configMonitorMock = new Mock<IOptionsMonitor<Service>>();
            _handlerMock = new Mock<HttpMessageHandler>();

            // Configure the options with valid values
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "health"
            };
            _configMonitorMock.Setup(x => x.Get("HealthCheckService")).Returns(config);
        }

        private HttpOnlinePaymentServiceHealthCheckService CreateService(HttpClient httpClient)
        {
            return new HttpOnlinePaymentServiceHealthCheckService(
                httpClient,
                _httpContextAccessorMock.Object,
                _configMonitorMock.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_ShouldInitializeInstance()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act
            var service = CreateService(httpClient);

            // Assert
            service.Should().NotBeNull();
            service.Should().BeAssignableTo<IHttpPaymentServiceHealthCheckService>();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenHttpContextAccessorIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act
            Action act = () => new HttpOnlinePaymentServiceHealthCheckService(httpClient, null!, _configMonitorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("httpContextAccessor");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenConfigIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act
            Action act = () => new HttpOnlinePaymentServiceHealthCheckService(httpClient, _httpContextAccessorMock.Object, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("config");
        }

        [TestMethod, AutoMoqData]
        public async Task GetHealthAsync_WithValidResponse_ReturnsHttpResponseMessage()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var healthCheckJson = @"{ ""status"": ""Healthy"" }";
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(healthCheckJson, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(_handlerMock.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.GetHealthAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Should().NotBeNull();
        }

        [TestMethod, AutoMoqData]
        public async Task GetHealthAsync_WhenHttpRequestFails_ThrowsHttpRequestException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_handlerMock.Object);
            var service = CreateService(httpClient);

            // Act
            Func<Task> act = async () => await service.GetHealthAsync(cancellationToken);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Network error");
        }

        [TestMethod, AutoMoqData]
        public async Task GetHealthAsync_WithUnsuccessfulResponse_ThrowsHttpRequestException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Internal Server Error")
                });

            var httpClient = new HttpClient(_handlerMock.Object);
            var service = CreateService(httpClient);

            // Act
            Func<Task> act = async () => await service.GetHealthAsync(cancellationToken);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}