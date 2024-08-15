using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Payment.Facade.Common.UnitTests.RESTServices
{
    [TestClass]
    public class HttpGovPayServiceTests
    {
        private Mock<IHttpContextAccessor>? _httpContextAccessorMock;
        private Mock<IOptions<Service>>? _configMock;
        private GovPayResponseDto? _expectedResponse;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "payments",
                BearerToken = "dummyBearerToken",
                Retries = 3
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
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_ShouldRetryOnFailure(
            [Frozen] GovPayRequestDto _govPayRequest,
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] CancellationToken _cancellationToken,
            [Frozen] HttpRequestException _mockException,
            [Frozen] GovPayResponseDto _mockResponse)
        {
            // Arrange
            _mockResponse.PaymentId = "12345";
            var postMethodCallCount = 0;
            var service = new Mock<HttpGovPayService>(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                _configMock!.Object)
            {
                CallBase = true // Use actual implementation for non-mocked methods
            };

            service.Protected()
                   .Setup<Task<GovPayResponseDto>>("Post", 
                        [typeof(GovPayResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<Object?>(), ItExpr.IsAny<CancellationToken>())
                   .Callback(() => postMethodCallCount++)
                   .ReturnsAsync(() =>
                   {
                       if (postMethodCallCount < 3) throw _mockException; // Throw exception on the first two calls
                       return _mockResponse; // Return mockResponse on the third call
                   });

            // Act
            var result = await service.Object.InitiatePaymentAsync(_govPayRequest, _cancellationToken);

            // Assert
            using(new AssertionScope())
            {
                result.Should().BeEquivalentTo(_mockResponse);
                postMethodCallCount.Should().Be(3); // Retries twice, succeeds on third attempt
            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_ShouldThrowAfterExhaustingRetries(
            [Frozen] GovPayRequestDto _govPayRequest,
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] CancellationToken _cancellationToken,
            [Frozen] HttpRequestException _mockException)
        {
            // Arrange
            var service = new Mock<HttpGovPayService>(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                _configMock!.Object)
            {
                CallBase = true // Use actual implementation for non-mocked methods
            };

            service.Protected()
                   .Setup<Task<GovPayResponseDto>>("Post",
                        [typeof(GovPayResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<Object?>(), ItExpr.IsAny<CancellationToken>())
                    .ThrowsAsync(_mockException);

            // Act
            Func<Task> act = async () => await service.Object.InitiatePaymentAsync(_govPayRequest, _cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorInitiatingPayment);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_ShouldNotRetryOnSuccess(
            [Frozen] GovPayRequestDto _govPayRequest,
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] CancellationToken _cancellationToken,
            [Frozen] GovPayResponseDto _mockResponse)
        {
            // Arrange
            _mockResponse.PaymentId = "12345";
            var postMethodCallCount = 0;
            var service = new Mock<HttpGovPayService>(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                _configMock!.Object)
            {
                CallBase = true // Use actual implementation for non-mocked methods
            };

            service.Protected()
                   .Setup<Task<GovPayResponseDto>>("Post",
                        [typeof(GovPayResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<Object?>(), ItExpr.IsAny<CancellationToken>())
                   .Callback(() => postMethodCallCount++)
                   .ReturnsAsync(_mockResponse);

            // Act
            var result = await service.Object.InitiatePaymentAsync(_govPayRequest, _cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(_mockResponse);
                postMethodCallCount.Should().Be(1); // No retries
            }
        }



        private HttpGovPayService CreateHttpGovPayService(HttpClient httpClient)
        {
            return new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_Success_ReturnsPaymentResponseDto(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            GovPayRequestDto paymentRequestDto,
            HttpGovPayService httpGovPayService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                       ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_expectedResponse)),
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpGovPayService = CreateHttpGovPayService(httpClient);

            // Act
            var result = await httpGovPayService.InitiatePaymentAsync(paymentRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PaymentId.Should().Be(_expectedResponse!.PaymentId);
                result.Amount.Should().Be(_expectedResponse.Amount);
                result.Reference.Should().Be(_expectedResponse.Reference);
                result.Description.Should().Be(_expectedResponse.Description);
                result.ReturnUrl.Should().Be(_expectedResponse.ReturnUrl);

                handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());

            }
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePayment_Failure_ThrowsException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            GovPayRequestDto paymentRequestDto,
            HttpGovPayService httpGovPayService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorInitiatingPayment));

            var httpClient = new HttpClient(handlerMock.Object);
            httpGovPayService = CreateHttpGovPayService(httpClient);

            // Act
            Func<Task> act = async () => await httpGovPayService.InitiatePaymentAsync(paymentRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorInitiatingPayment);

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Exactly(4),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatus_Success_ReturnsPaymentStatusResponseDto(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            string paymentId,
            HttpGovPayService httpGovPayService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                       ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(new PaymentStatusResponseDto() 
                           { State = new Dtos.Response.Payments.Common.State() { Status = "InProgress" } })),
                       }).Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            httpGovPayService = CreateHttpGovPayService(httpClient);

            // Act
            var result = await httpGovPayService.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());

            };
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatus_Failure_ThrowsException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            string paymentId,
            HttpGovPayService httpGovPayService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorRetrievingPaymentStatus));

            var httpClient = new HttpClient(handlerMock.Object);
            httpGovPayService = CreateHttpGovPayService(httpClient);

            // Act
            Func<Task> act = async () => await httpGovPayService.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Exactly(4),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public void GetPaymentStatusAsync_WhenBearerTokenIsNull_ThrowsArgumentNullException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            string paymentId,
            CancellationToken cancellationToken)
        {
            // Arrange
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "payments",
                BearerToken = null // Simulate null BearerToken
            };
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(x => x.Value).Returns(config);

            // Act
            Action act = () => new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(new HttpClient(handlerMock.Object)),
                configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithMessage("*Bearer token is null. Unable to initiate payment.*")
               .WithParameterName("config");
        }
    }
}


