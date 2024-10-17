using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer;
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
    public class HttpProducerResubmissionFeesServiceTests
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
                ApplicationReferenceNumber = "A123"
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

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ValidRequest_ReturnsRegistrationFee(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Greedy] HttpProducerResubmissionFeesService httpProducerResubmissionFeesService,
            [Frozen] RegulatorDto request,
            [Frozen] decimal expectedAmount,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(expectedAmount), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerResubmissionFeesService = CreateHttpProducerRegistrationFeesService(httpClient);

            // Act
            var result = await httpProducerResubmissionFeesService.GetResubmissionFeeAsync(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().Be(expectedAmount);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_HttpRequestException_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Mock<IOptions<Service>> configMock,
            [Greedy] HttpProducerResubmissionFeesService httpProducerResubmissionFeesService,
            [Frozen] RegulatorDto request,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Unexpected error"));

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerResubmissionFeesService = CreateHttpProducerRegistrationFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpProducerResubmissionFeesService.GetResubmissionFeeAsync(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                    .WithMessage(ExceptionMessages.ErrorResubmissionFees);


                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_UnsuccessfulStatusCode_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Mock<IOptions<Service>> configMock,
            [Greedy] HttpProducerResubmissionFeesService httpProducerResubmissionFeesService,
            [Frozen] RegulatorDto request,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.BadRequest, // Simulate unsuccessful status code
                           Content = new StringContent(JsonConvert.SerializeObject("GB-ENG"), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerResubmissionFeesService = CreateHttpProducerRegistrationFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpProducerResubmissionFeesService.GetResubmissionFeeAsync(request, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorResubmissionFees);

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        private HttpProducerResubmissionFeesService CreateHttpProducerRegistrationFeesService(HttpClient httpClient)
        {
            return new HttpProducerResubmissionFeesService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }
    }
}
