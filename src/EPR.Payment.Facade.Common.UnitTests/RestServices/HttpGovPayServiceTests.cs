using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments.Common;
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
        public void Constructor_WithValidParameters_ShouldNotThrowException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var serviceOptions = new Service
            {
                Url = "https://valid-url.com",
                EndPointName = "ValidEndPoint",
                BearerToken = "ValidToken"
            };

            _configMock!.Setup(x => x.Value).Returns(serviceOptions);
            // Act
            Action act = () => new HttpGovPayService(_httpContextAccessorMock!.Object, new HttpClientFactoryMock(new HttpClient(handlerMock.Object)), _configMock.Object);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullHttpContextAccessor_ShouldThrowArgumentNullException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var serviceOptions = new Service
            {
                Url = "https://valid-url.com",
                EndPointName = "ValidEndPoint",
                BearerToken = "ValidToken"
            };
            _configMock!.Setup(o => o.Value).Returns(serviceOptions);

            // Act
            Action act = () => new HttpGovPayService(null!, new HttpClientFactoryMock(new HttpClient(handlerMock.Object)), _configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*httpContextAccessor*");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenBearerTokenIsNull_ThrowsArgumentNullException(
                            [Frozen] Mock<HttpMessageHandler> handlerMock)
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

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullUrl_ShouldThrowArgumentNullException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var config = new Service
            {
                Url = null, // Url is null
                EndPointName = "ValidEndPoint",
                BearerToken = "ValidToken"
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
               .WithParameterName("config");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullEndPointName_ShouldThrowArgumentNullException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var config = new Service
            {
                Url = "https://valid-url.com",
                EndPointName = null, // EndPointName is null
                BearerToken = "ValidToken"
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
               .WithParameterName("config");
        }


        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_ShouldRetryOnFailure(
            [Frozen] GovPayRequestDto _govPayRequest,
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] HttpRequestException _mockException,
            [Frozen] GovPayResponseDto _mockResponse,
            [Frozen] CancellationToken _cancellationToken)
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
            using (new AssertionScope())
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
            [Frozen] HttpRequestException _mockException,
            [Frozen] CancellationToken _cancellationToken)
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
            await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorInitiatingPayment);
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_ShouldNotRetryOnSuccess(
            [Frozen] GovPayRequestDto _govPayRequest,
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] GovPayResponseDto _mockResponse,
            [Frozen] CancellationToken _cancellationToken)
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

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_ShouldRetryOnFailure(
           [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
           [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
           [Frozen] HttpRequestException _mockException,
           [Frozen] PaymentStatusResponseDto _mockResponse,
           string _paymentId,
           [Frozen] CancellationToken _cancellationToken)
        {
            // Arrange
            _mockResponse.PaymentId = _paymentId;
            var postMethodCallCount = 0;
            var service = new Mock<HttpGovPayService>(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                _configMock!.Object)
            {
                CallBase = true // Use actual implementation for non-mocked methods
            };

            service.Protected()
                   .Setup<Task<PaymentStatusResponseDto>>("Get",
                        [typeof(PaymentStatusResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<CancellationToken>(), ItExpr.IsAny<bool>())
                   .Callback(() => postMethodCallCount++)
                   .ReturnsAsync(() =>
                   {
                       if (postMethodCallCount < 3) throw _mockException; // Throw exception on the first two calls
                       return _mockResponse; // Return mockResponse on the third call
                   });

            // Act
            var result = await service.Object.GetPaymentStatusAsync(_paymentId, _cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(_mockResponse);
                postMethodCallCount.Should().Be(3); // Retries twice, succeeds on third attempt
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_WhenResponseStatusIsNull_ShouldRetryOnFailure(
           [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
           [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
           [Frozen] HttpRequestException _mockException,
           [Frozen] PaymentStatusResponseDto _mockResponse,
           string _paymentId,
           [Frozen] CancellationToken _cancellationToken)
        {
            // Arrange
            _mockResponse.PaymentId = _paymentId;
            var postMethodCallCount = 0;
            var service = new Mock<HttpGovPayService>(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                _configMock!.Object)
            {
                CallBase = true // Use actual implementation for non-mocked methods
            };

            service.Protected()
                   .Setup<Task<PaymentStatusResponseDto>>("Get",
                        [typeof(PaymentStatusResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<CancellationToken>(), ItExpr.IsAny<bool>())
                   .Callback(() => postMethodCallCount++)
                   .ReturnsAsync(() =>
                   {
                       if (postMethodCallCount < 3)
                           return new PaymentStatusResponseDto() { State = new State() { Status = null } }; // Throw exception on the first two calls
                       return _mockResponse; // Return mockResponse on the third call
                   });

            // Act
            var result = await service.Object.GetPaymentStatusAsync(_paymentId, _cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(_mockResponse);
                postMethodCallCount.Should().Be(3); // Retries twice, succeeds on third attempt
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_WhenResponseStatusIsEmpty_ShouldRetryOnFailure(
           [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
           [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
           [Frozen] HttpRequestException _mockException,
           [Frozen] PaymentStatusResponseDto _mockResponse,
           string _paymentId,
           [Frozen] CancellationToken _cancellationToken)
        {
            // Arrange
            _mockResponse.PaymentId = _paymentId;
            var postMethodCallCount = 0;
            var service = new Mock<HttpGovPayService>(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                _configMock!.Object)
            {
                CallBase = true // Use actual implementation for non-mocked methods
            };

            service.Protected()
                   .Setup<Task<PaymentStatusResponseDto>>("Get",
                        [typeof(PaymentStatusResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<CancellationToken>(), ItExpr.IsAny<bool>())
                   .Callback(() => postMethodCallCount++)
                   .ReturnsAsync(() =>
                   {
                       if (postMethodCallCount < 3)
                           return new PaymentStatusResponseDto() { State = new State() { Status = string.Empty } }; // Throw exception on the first two calls
                       return _mockResponse; // Return mockResponse on the third call
                   });

            // Act
            var result = await service.Object.GetPaymentStatusAsync(_paymentId, _cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(_mockResponse);
                postMethodCallCount.Should().Be(3); // Retries twice, succeeds on third attempt
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync__WhenResponseIsNull_ShouldRetryOnFailure(
           [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
           [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
           [Frozen] PaymentStatusResponseDto _mockResponse,
           string _paymentId,
           [Frozen] CancellationToken _cancellationToken)
        {
            // Arrange
            _mockResponse.PaymentId = _paymentId;
            var postMethodCallCount = 0;
            var service = new Mock<HttpGovPayService>(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                _configMock!.Object)
            {
                CallBase = true // Use actual implementation for non-mocked methods
            };

            service.Protected()
                   .Setup<Task<PaymentStatusResponseDto>>("Get",
                        [typeof(PaymentStatusResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<CancellationToken>(), ItExpr.IsAny<bool>())
                   .Callback(() => postMethodCallCount++)
                   .ReturnsAsync(() =>
                   {
                       if (postMethodCallCount < 3) return null!;
                       return _mockResponse; // Return mockResponse on the third call
                   });

            // Act
            var result = await service.Object.GetPaymentStatusAsync(_paymentId, _cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(_mockResponse);
                postMethodCallCount.Should().Be(3); // Retries twice, succeeds on third attempt
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_ShouldThrowAfterExhaustingRetries(
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] HttpRequestException _mockException,
            [Frozen] PaymentStatusResponseDto _mockResponse,
            string _paymentId,
            [Frozen] CancellationToken _cancellationToken)
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
                   .Setup<Task<PaymentStatusResponseDto>>("Get",
                        [typeof(PaymentStatusResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<CancellationToken>(), ItExpr.IsAny<bool>())
                   .ThrowsAsync(_mockException);

            // Act
            Func<Task> act = async () => await service.Object.GetPaymentStatusAsync(_paymentId, _cancellationToken);

            // Assert
            await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);

        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_ShouldNotRetryOnSuccess(
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] PaymentStatusResponseDto _mockResponse,
            string _paymentId,
            [Frozen] CancellationToken _cancellationToken)
        {
            // Arrange
            _mockResponse.PaymentId = _paymentId;
            var postMethodCallCount = 0;
            var service = new Mock<HttpGovPayService>(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                _configMock!.Object)
            {
                CallBase = true // Use actual implementation for non-mocked methods
            };

            service.Protected()
                   .Setup<Task<PaymentStatusResponseDto>>("Get",
                        [typeof(PaymentStatusResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<CancellationToken>(), ItExpr.IsAny<bool>())
                    .Callback(() => postMethodCallCount++)
                    .ReturnsAsync(_mockResponse);

            // Act
            var result = await service.Object.GetPaymentStatusAsync(_paymentId, _cancellationToken);

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

        [TestMethod, AutoMoqData]
        public void Constructor_ShouldInitializeDependencies(
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] Service _serviceConfig,
            [Frozen] IOptions<Service> _configOptions)
        {
            // Arrange
            _configOptions = Options.Create(_serviceConfig);

            // Act
            var service = new HttpGovPayService(_httpContextAccessorMock.Object, _httpClientFactoryMock.Object, _configOptions);

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenHttpContextAccessorIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] Service _serviceConfig,
            [Frozen] IOptions<Service> _configOptions)
        {
            // Arrange
            _configOptions = Options.Create(_serviceConfig);

            // Act
            Action act = () => new HttpGovPayService(null!, _httpClientFactoryMock.Object, _configOptions);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*httpContextAccessor*");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenHttpClientFactoryIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Service _serviceConfig,
            [Frozen] IOptions<Service> _configOptions)
        {
            // Arrange
            _configOptions = Options.Create(_serviceConfig);

            // Act
            Action act = () => new HttpGovPayService(_httpContextAccessorMock.Object, null!, _configOptions);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*httpClientFactory*");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenUrlConfigIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] Service _serviceConfig,
            [Frozen] IOptions<Service> _configOptions)
        {
            // Arrange
            _configOptions = Options.Create(_serviceConfig);
            _configOptions.Value.Url = null!;

            // Act
            Action act = () => new HttpGovPayService(_httpContextAccessorMock.Object, _httpClientFactoryMock.Object, _configOptions);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*config*");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenEndPointNameConfigIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] Service _serviceConfig,
            [Frozen] IOptions<Service> _configOptions)
        {
            // Arrange
            _configOptions = Options.Create(_serviceConfig);
            _configOptions.Value.EndPointName = null!;

            // Act
            Action act = () => new HttpGovPayService(_httpContextAccessorMock.Object, _httpClientFactoryMock.Object, _configOptions);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*config*");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_WhenTokenIsNotNull_ShouldSetAuthorizationHeader(
            [Frozen] Mock<IHttpContextAccessor> _httpContextAccessorMock,
            [Frozen] Mock<HttpMessageHandler> _httpMessageHandlerMock,
            [Frozen] Mock<IHttpClientFactory> _httpClientFactoryMock,
            [Frozen] GovPayRequestDto _paymentRequestDto,
            [Frozen] Service _serviceConfig,
            [Frozen] IOptions<Service> _configOptions,
            [Frozen] GovPayResponseDto _mockResponse)
        {
            // Arrange
            var postMethodCallCount = 0;
            _mockResponse.PaymentId = "12345";
            _serviceConfig.Url = "http://example.com";
            _serviceConfig.EndPointName = "test";
            _serviceConfig.BearerToken = "valid_token";
            _configOptions = Options.Create(_serviceConfig);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                                  .Returns(httpClient);

            var httpGovPayService = new Mock<HttpGovPayService>(
                _httpContextAccessorMock.Object,
                _httpClientFactoryMock.Object,
                _configOptions)        
            {
                CallBase = true
            };

            httpGovPayService.Protected()
           .Setup<Task<GovPayResponseDto>>("Post",
                [typeof(GovPayResponseDto)], true, ItExpr.IsAny<string>(), ItExpr.IsAny<Object?>(), ItExpr.IsAny<CancellationToken>())
           .Callback(() => postMethodCallCount++)
           .ReturnsAsync(() =>
           {
               return _mockResponse;
           });

            // Act
            await httpGovPayService.Object.InitiatePaymentAsync(_paymentRequestDto, CancellationToken.None);

            // Assert
            using(new AssertionScope())
            {
                httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
                httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
                httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be("valid_token");
            }

        }

    }
}


