using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;
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
        private ProducerResubmissionFeeRequestDto _requestDto = null!;
        private ProducerResubmissionFeeResponseDto _responseDto = null!;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://api.example.com",
                EndPointName = "producer/resubmission-fee",
                HttpClientName = "HttpClientName"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _requestDto = new ProducerResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow,
                ReferenceNumber = "PROD-REF-1234"
            };

            _responseDto = new ProducerResubmissionFeeResponseDto
            {
                TotalResubmissionFee = 1000,
                PreviousPayments = 200,
                OutstandingPayment = 800
            };
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ValidRequest_ReturnsResubmissionFee(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Greedy] HttpProducerResubmissionFeesService httpProducerResubmissionFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_responseDto), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerResubmissionFeesService = CreateHttpProducerResubmissionFeesService(httpClient);

            // Act
            var result = await httpProducerResubmissionFeesService.GetResubmissionFeeAsync(_requestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(_responseDto);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_HttpRequestException_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Greedy] HttpProducerResubmissionFeesService httpProducerResubmissionFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Unexpected error"));

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerResubmissionFeesService = CreateHttpProducerResubmissionFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpProducerResubmissionFeesService.GetResubmissionFeeAsync(_requestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                    .WithMessage(ExceptionMessages.ErrorResubmissionFees);

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_UnsuccessfulStatusCode_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Greedy] HttpProducerResubmissionFeesService httpProducerResubmissionFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.BadRequest, // Simulate unsuccessful status code
                           Content = new StringContent("Error message", Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerResubmissionFeesService = CreateHttpProducerResubmissionFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpProducerResubmissionFeesService.GetResubmissionFeeAsync(_requestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                    .WithMessage(ExceptionMessages.ErrorResubmissionFees);

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        private HttpProducerResubmissionFeesService CreateHttpProducerResubmissionFeesService(HttpClient httpClient)
        {
            return new HttpProducerResubmissionFeesService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }
    }
}
