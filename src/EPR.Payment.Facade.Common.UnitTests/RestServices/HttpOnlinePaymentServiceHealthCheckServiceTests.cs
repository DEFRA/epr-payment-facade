using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
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

        [TestMethod]
        public void Constructor_ShouldInitializeInstance()
        {
            // Arrange
            var config = new Service
            {
                Url = "https://example.com",   // Properly mock the URL
                EndPointName = "health"
            };

            // Mocking IOptionsMonitor<Service> to return the configuration
            _configMonitorMock.Setup(x => x.Get("ProducerFeesService")).Returns(config);

            var httpClient = new HttpClient();

            // Act
            var service = CreateService(httpClient);

            // Assert
            service.Should().NotBeNull();
            service.Should().BeAssignableTo<IHttpPaymentServiceHealthCheckService>();
        }

        [TestMethod]
        public async Task GetHealthAsync_WithValidResponse_ReturnsHttpResponseMessage()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var healthCheckJson = @"{ ""status"": ""Healthy"" }";

            // Mock the Service configuration with valid values
            var serviceConfig = new Service
            {
                Url = "https://example.com",   // Properly mock the URL
                EndPointName = "health"
            };

            // Mock IOptionsMonitor to return the configured service
            _configMonitorMock.Setup(x => x.Get("ProducerFeesService")).Returns(serviceConfig);

            // Set up the mock HttpMessageHandler to return a valid response
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


        [TestMethod]
        public async Task GetHealthAsync_WhenHttpRequestFails_ThrowsHttpRequestException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Set up the mock for IOptionsMonitor<Service>
            var serviceConfig = new Service
            {
                Url = "https://example.com",   // Mock the URL here
                EndPointName = "health"
            };

            // Mock IOptionsMonitor to return the configured service
            _configMonitorMock.Setup(x => x.Get("ProducerFeesService")).Returns(serviceConfig);

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

        [TestMethod]
        public async Task GetHealthAsync_WithUnsuccessfulResponse_ThrowsResponseCodeException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Mock the Service configuration
            var serviceConfig = new Service
            {
                Url = "https://example.com",  // Mock the URL here
                EndPointName = "health"
            };

            // Mock IOptionsMonitor to return the configured service
            _configMonitorMock.Setup(x => x.Get("ProducerFeesService")).Returns(serviceConfig);

            // Setup the mock for HttpMessageHandler to return an unsuccessful response
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
            await act.Should().ThrowAsync<ResponseCodeException>()
                .WithMessage("Internal Server Error");  // You can adjust this based on the actual exception message
        }


    }
}
