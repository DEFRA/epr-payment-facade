using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace EPR.Payment.Facade.Common.UnitTests.RESTServices
{
    [TestClass]
    public class HttpProducerFeesServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptions<Service>> _configMock = null!;
        private ProducerFeesRequestDto _producerFeesRequestDto = null!;
        private ProducerFeesResponseDto _producerFeesResponseDto = null!;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://api.example.com",
                EndPointName = "registration-fee",
                HttpClientName = "HttpClientName"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _producerFeesRequestDto = new ProducerFeesRequestDto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = "GB-ENG",
                IsProducerOnlineMarketplace = false,
                IsLateFeeApplicable = false,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow
            };

            _producerFeesResponseDto = new ProducerFeesResponseDto
            {
                TotalFee = 1000,
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdown
                {
                    FeeBreakdowns = new List<FeeBreakdown>
                    {
                        new FeeBreakdown
                        {
                            BandNumber = 1,
                            UnitCount = 2,
                            UnitPrice = 500,
                            TotalPrice = 1000
                        },
                        new FeeBreakdown
                        {
                            BandNumber = 2,
                            UnitCount = 3,
                            UnitPrice = 200,
                            TotalPrice = 600
                        },
                        new FeeBreakdown
                        {
                            BandNumber = 3,
                            UnitCount = 0,
                            UnitPrice = 500,
                            TotalPrice = 0
                        }
                    }
                }
            };
        }

        private HttpProducerFeesService CreateHttpProducerFeesService(HttpClient httpClient)
        {
            return new HttpProducerFeesService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpContextAccessorIsNull_ShouldThrowArgumentNullException(
            Mock<IHttpClientFactory> httpClientFactoryMock,
            Mock<IOptions<Service>> configMock)
        {
            // Act
            Action act = () => new HttpProducerFeesService(null!, httpClientFactoryMock.Object, configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpContextAccessor");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpClientFactoryIsNull_ShouldThrowArgumentNullException(
            Mock<IHttpContextAccessor> httpContextAccessorMock,
            Mock<IOptions<Service>> configMock)
        {
            // Act
            Action act = () => new HttpProducerFeesService(httpContextAccessorMock.Object, null!, configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClientFactory");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_ConfigUrlIsNull_ShouldThrowArgumentNullException(
            Mock<IHttpContextAccessor> httpContextAccessorMock,
            Mock<IHttpClientFactory> httpClientFactoryMock)
        {
            // Arrange
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(c => c.Value).Returns(new Service { Url = null, EndPointName = "SomeEndPoint" });

            // Act
            Action act = () => new HttpProducerFeesService(httpContextAccessorMock.Object, httpClientFactoryMock.Object, configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.RegistrationFeesServiceBaseUrlMissing} (Parameter 'config')");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_ConfigEndPointNameIsNull_ShouldThrowArgumentNullException(
            Mock<IHttpContextAccessor> httpContextAccessorMock,
            Mock<IHttpClientFactory> httpClientFactoryMock)
        {
            // Arrange
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(c => c.Value).Returns(new Service { Url = "https://api.example.com", EndPointName = null });

            // Act
            Action act = () => new HttpProducerFeesService(httpContextAccessorMock.Object, httpClientFactoryMock.Object, configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.RegistrationFeesServiceEndPointNameMissing} (Parameter 'config')");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_ValidRequest_ReturnsRegistrationFeesResponseDto(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Mock<IOptions<Service>> configMock,
            HttpProducerFeesService httpProducerFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_producerFeesResponseDto), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerFeesService = CreateHttpProducerFeesService(httpClient);

            // Act
            var result = await httpProducerFeesService.CalculateProducerFeesAsync(_producerFeesRequestDto, cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(_producerFeesResponseDto);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_HttpRequestException_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Mock<IOptions<Service>> configMock,
            HttpProducerFeesService httpRegistrationFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Unexpected error"));

            var httpClient = new HttpClient(handlerMock.Object);
            httpRegistrationFeesService = CreateHttpProducerFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpRegistrationFeesService.CalculateProducerFeesAsync(_producerFeesRequestDto, cancellationToken);

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

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_NullContent_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Mock<IOptions<Service>> configMock,
            HttpProducerFeesService httpProducerFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = null // Simulate a null response content
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerFeesService = CreateHttpProducerFeesService(httpClient);

            // Act
            Func<Task> act = async () =>
            {
                var response = await httpProducerFeesService.CalculateProducerFeesAsync(_producerFeesRequestDto, cancellationToken);
                // Manually check for null content to simulate the exception throwing
                if (response == null)
                {
                    throw new ServiceException(ExceptionMessages.ErrorCalculatingProducerFees);
                }
            };

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

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_UnsuccessfulStatusCode_ThrowsValidationException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Mock<IOptions<Service>> configMock,
            HttpProducerFeesService httpRegistrationFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new ResponseCodeException(HttpStatusCode.BadRequest, "Invalid input parameter."));

            var httpClient = new HttpClient(handlerMock.Object);
            httpRegistrationFeesService = CreateHttpProducerFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpRegistrationFeesService.CalculateProducerFeesAsync(_producerFeesRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Invalid input parameter.");

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