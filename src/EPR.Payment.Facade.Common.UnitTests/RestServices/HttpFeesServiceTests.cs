using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Common.RESTServices;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Tests.Services
{
    [TestClass]
    public class HttpFeesServiceTests
    {
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private HttpFeesService _httpFeesService;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            _httpFeesService = new HttpFeesService(_mockHttpContextAccessor.Object, _mockHttpClientFactory.Object, "https://baseurl");
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldReturnFee_WhenRequestIsValid()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                PayBaseFee = true
            };

            var expectedResponse = new RegistrationFeeResponseDto { TotalFee = 1000 };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse))
                });

            // Act
            var result = await _httpFeesService.CalculateProducerFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldReturnFee_WhenRequestIsValid()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                Producers = new System.Collections.Generic.List<ProducerSubsidiaryInfo>
                {
                    new ProducerSubsidiaryInfo { ProducerType = "L", NumberOfSubsidiaries = 10, PayBaseFee = true },
                    new ProducerSubsidiaryInfo { ProducerType = "S", NumberOfSubsidiaries = 5, PayBaseFee = true }
                },
                PayComplianceSchemeBaseFee = true
            };

            var expectedResponse = new RegistrationFeeResponseDto { TotalFee = 3000 };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse))
                });

            // Act
            var result = await _httpFeesService.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
