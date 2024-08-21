using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
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

namespace EPR.Payment.Facade.Common.UnitTests.RESTServices.RegistrationFees
{
    [TestClass]
    public class HttpRegistrationFeesServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptions<Service>> _configMock = null!;
        private ProducerRegistrationFeesRequestDto _producerRegistrationFeesRequestDto = null!;
        private RegistrationFeesResponseDto _registrationFeesResponseDto = null!;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://api.example.com",
                EndPointName = "registration-fees",
                HttpClientName = "HttpClientName"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _producerRegistrationFeesRequestDto = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                Regulator = "GB-ENG",
                IsOnlineMarketplace = false,
                PayBaseFee = true
            };

            _registrationFeesResponseDto = new RegistrationFeesResponseDto
            {
                TotalFee = 1000,
                FeeBreakdowns = new List<FeeBreakdown>
                {
                    new FeeBreakdown
                    {
                        Description = "Base Fee",
                        Amount = 500
                    },
                    new FeeBreakdown
                    {
                        Description = "Subsidiaries Fee",
                        Amount = 500
                    }
                }
            };
        }

        private HttpRegistrationFeesService CreateHttpRegistrationFeesService(HttpClient httpClient)
        {
            return new HttpRegistrationFeesService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_Success_ReturnsRegistrationFeesResponseDto(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpRegistrationFeesService httpRegistrationFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_registrationFeesResponseDto), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpRegistrationFeesService = CreateHttpRegistrationFeesService(httpClient);

            // Act
            var result = await httpRegistrationFeesService.CalculateProducerFeesAsync(_producerRegistrationFeesRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(_registrationFeesResponseDto);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_Failure_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpRegistrationFeesService httpRegistrationFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Unexpected error"));

            var httpClient = new HttpClient(handlerMock.Object);
            httpRegistrationFeesService = CreateHttpRegistrationFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpRegistrationFeesService.CalculateProducerFeesAsync(_producerRegistrationFeesRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                    .WithMessage(ExceptionMessages.ErrorCalculatingProducerFees);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }
    }
}
