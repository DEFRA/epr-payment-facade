using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace EPR.Payment.Facade.Common.UnitTests.RestServices
{
    [TestClass]
    public class HttpComplianceSchemeFeesServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptions<Service>> _configMock = null!;
        private RegulatorDto _regulatorDto = null!;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "compliance-scheme",
                HttpClientName = "HttpClientName"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _regulatorDto = new RegulatorDto
            {
                Regulator = "GB-ENG"
            };
        }

        private HttpComplianceSchemeFeesService CreateHttpComplianceSchemeFeesService(HttpClient httpClient)
        {
            return new HttpComplianceSchemeFeesService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public async Task GetComplianceSchemeBaseFeeAsync_Success_ReturnsBaseFeeResponseDto(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpComplianceSchemeFeesService httpComplianceSchemeFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            var baseFeeResponse = new ComplianceSchemeBaseFeeResponse { BaseFee = 1380400m };

            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(baseFeeResponse), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpComplianceSchemeFeesService = CreateHttpComplianceSchemeFeesService(httpClient);

            // Act
            var result = await httpComplianceSchemeFeesService.GetComplianceSchemeBaseFeeAsync(_regulatorDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(baseFeeResponse);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task GetComplianceSchemeBaseFeeAsync_InvalidBaseFee_ShouldThrowServiceException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var configMock = Options.Create(new Service
            {
                Url = "https://example.com",
                EndPointName = "compliance-scheme-fees"
            });

            var service = new HttpComplianceSchemeFeesService(
                new HttpContextAccessor(),
                new HttpClientFactoryMock(httpClient),
                configMock);

            var request = new RegulatorDto { Regulator = "GB-ENG" };
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent("{\"BaseFee\": 0}")
                       });

            // Act
            Func<Task> action = async () => await service.GetComplianceSchemeBaseFeeAsync(request);

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage("An unexpected error occurred while retrieving the compliance scheme base fee.");
        }


        [TestMethod, AutoMoqData]
        public async Task GetComplianceSchemeBaseFeeAsync_HttpRequestException_ShouldThrowServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpComplianceSchemeFeesService httpComplianceSchemeFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee));

            var httpClient = new HttpClient(handlerMock.Object);
            httpComplianceSchemeFeesService = CreateHttpComplianceSchemeFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpComplianceSchemeFeesService.GetComplianceSchemeBaseFeeAsync(_regulatorDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                    .WithMessage(ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee);

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod]
        public async Task GetComplianceSchemeBaseFeeAsync_UnexpectedException_ShouldThrowServiceException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var configMock = Options.Create(new Service
            {
                Url = "https://example.com",
                EndPointName = "compliance-scheme-fees"
            });

            var service = new HttpComplianceSchemeFeesService(
                new HttpContextAccessor(),
                new HttpClientFactoryMock(httpClient),
                configMock);

            var request = new RegulatorDto { Regulator = "GB-ENG" };
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .Throws(new Exception("Test exception"));

            // Act
            Func<Task> action = async () => await service.GetComplianceSchemeBaseFeeAsync(request);

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage("An unexpected error occurred while retrieving the compliance scheme base fee.");
        }

        [TestMethod]
        public async Task GetComplianceSchemeBaseFeeAsync_RequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = CreateHttpComplianceSchemeFeesService(httpClient);

            // Act
            Func<Task> act = async () => await service.GetComplianceSchemeBaseFeeAsync(null!, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee} (Parameter 'request')");
        }
    }
}
