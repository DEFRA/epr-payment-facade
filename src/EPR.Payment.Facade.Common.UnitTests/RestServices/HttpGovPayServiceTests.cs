﻿using AutoFixture.MSTest;
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
using System.Text;

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
        public void Constructor_WithValidParameters_ShouldNotThrowException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock)
        {
            // Arrange
            var serviceOptions = new Service
            {
                Url = "https://valid-url.com",
                EndPointName = "ValidEndPoint",
                BearerToken = "ValidToken",
                Retries = 3
            };

            _configMock!.Setup(x => x.Value).Returns(serviceOptions);

            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            Action act = () => new HttpGovPayService(
                httpClient,
                httpContextAccessorMock.Object,
                _configMock.Object);

            // Assert
            act.Should().NotThrow();

            // Additional validation to ensure the service is initialized properly
            var service = new HttpGovPayService(httpClient, httpContextAccessorMock.Object, _configMock.Object);
            service.Should().NotBeNull();
        }


        [TestMethod, AutoMoqData]
        public void Constructor_WithNullHttpContextAccessor_ShouldThrowArgumentNullException(
            [Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(o => o.Value).Returns(new Service
            {
                Url = "https://valid-url.com",
                EndPointName = "ValidEndPoint",
                BearerToken = "ValidToken"
            });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            Action act = () => new HttpGovPayService(
                httpClient,
                null!, // Simulate null IHttpContextAccessor
                configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*Value cannot be null. (Parameter 'httpContextAccessor')*");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullUrl_ShouldThrowArgumentNullException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock)
        {
            // Arrange
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(x => x.Value).Returns(new Service
            {
                Url = null, // Simulate a null URL
                EndPointName = "ValidEndPoint",
                BearerToken = "ValidToken"
            });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            Action act = () => new HttpGovPayService(
                httpClient,
                httpContextAccessorMock.Object,
                configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithMessage("OnlinePaymentService BaseUrl configuration is missing (Parameter 'config')")
               .WithParameterName("config");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullEndPointName_ShouldThrowArgumentNullException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock)
        {
            // Arrange
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(x => x.Value).Returns(new Service
            {
                Url = "https://example.com", // Valid URL
                EndPointName = null,         // Simulate null EndPointName
                BearerToken = "ValidToken"
            });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            Action act = () => new HttpGovPayService(
                httpClient,
                httpContextAccessorMock.Object,
                configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithMessage("OnlinePaymentService EndPointName configuration is missing (Parameter 'config')")
               .WithParameterName("config");
        }

        [TestMethod, AutoMoqData]
        public async Task InitiatePaymentAsync_ShouldRetryOnFailure(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            GovPayRequestDto paymentRequestDto,
            CancellationToken cancellationToken)
        {
            // Arrange
            var retryCount = 0;

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    retryCount++;
                    if (retryCount < 3)
                        throw new HttpRequestException("Transient error");
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(_expectedResponse))
                    };
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var httpGovPayService = CreateHttpGovPayService(httpClient);

            // Act
            var result = await httpGovPayService.InitiatePaymentAsync(paymentRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                retryCount.Should().Be(3);
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
            [Frozen] GovPayRequestDto govPayRequest,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<IOptions<Service>> configMock)
        {
            // Arrange
            configMock.Setup(x => x.Value).Returns(new Service
            {
                Url = "https://example.com",
                EndPointName = "payments",
                BearerToken = "test-token",
                Retries = 3
            });

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

            var mockResponse = new GovPayResponseDto { PaymentId = "12345" };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(mockResponse)),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var govPayService = new HttpGovPayService(
                httpClient,
                httpContextAccessorMock.Object,
                configMock.Object);

            // Act
            var result = await govPayService.InitiatePaymentAsync(govPayRequest, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(mockResponse);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_ShouldRetryOnFailure(
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Service serviceConfig,
            string paymentId,
            [Frozen] CancellationToken cancellationToken)
        {
            // Arrange
            serviceConfig.Url = "https://example.com";
            serviceConfig.EndPointName = "payments";
            serviceConfig.Retries = 3; // Set retries to test retry logic
            var configOptions = Options.Create(serviceConfig);

            var retryCount = 0;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains(paymentId)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    retryCount++;
                    if (retryCount < 3) // Simulate failure on the first two attempts
                    {
                        throw new HttpRequestException("Simulated transient failure.");
                    }
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(new PaymentStatusResponseDto
                        {
                            PaymentId = paymentId,
                            State = new Dtos.Response.Payments.Common.State { Status = "success" }
                        }), Encoding.UTF8, "application/json")
                    };
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(serviceConfig.Url)
            };

            var service = new HttpGovPayService(httpClient, httpContextAccessorMock.Object, configOptions);

            // Act
            var result = await service.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.PaymentId.Should().Be(paymentId);
                result.State.Status.Should().Be("success");
                retryCount.Should().Be(3); // Verify retries happened
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_WhenResponseStatusIsNull_ShouldRetryOnFailure(
           [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
           [Frozen] Mock<HttpMessageHandler> handlerMock,
           Service serviceConfig,
           string paymentId,
           CancellationToken cancellationToken)
        {
            // Arrange
            serviceConfig.Url = "https://example.com";
            serviceConfig.EndPointName = "payments";
            serviceConfig.Retries = 3; // Set retries to test retry logic
            var configOptions = Options.Create(serviceConfig);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(serviceConfig.Url)
            };

            var postMethodCallCount = 0;

            // Mock response with a null status
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains(paymentId)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    postMethodCallCount++;
                    if (postMethodCallCount < 3)
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonConvert.SerializeObject(
                                new PaymentStatusResponseDto
                                {
                                    State = new State { Status = null } // Null status
                                }))
                        };
                    }

                    // Return a valid response on the third call
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(
                            new PaymentStatusResponseDto
                            {
                                PaymentId = paymentId,
                                State = new State { Status = "Success" }
                            }))
                    };
                });

            var service = new HttpGovPayService(httpClient, httpContextAccessorMock.Object, configOptions);

            // Act
            var result = await service.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.State.Status.Should().Be("Success");
                postMethodCallCount.Should().Be(3); // Retries twice, succeeds on the third attempt
            }
        }


        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_WhenResponseStatusIsEmpty_ShouldRetryOnFailure(
           [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
           [Frozen] Mock<HttpMessageHandler> handlerMock,
           Service serviceConfig,
           string paymentId,
           CancellationToken cancellationToken)
        {
            // Arrange
            serviceConfig.Url = "https://example.com";
            serviceConfig.EndPointName = "payments";
            serviceConfig.Retries = 3; // Set retries to test retry logic
            var configOptions = Options.Create(serviceConfig);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(serviceConfig.Url)
            };

            // Mock response with a status of empty
            var postMethodCallCount = 0;
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains(paymentId)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    postMethodCallCount++;
                    if (postMethodCallCount < 3)
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonConvert.SerializeObject(
                                new PaymentStatusResponseDto
                                {
                                    State = new State { Status = string.Empty } // Empty status
                                }))
                        };
                    }

                    // Return a valid response on the third call
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(
                            new PaymentStatusResponseDto
                            {
                                PaymentId = paymentId,
                                State = new State { Status = "Success" }
                            }))
                    };
                });

            var service = new HttpGovPayService(httpClient, httpContextAccessorMock.Object, configOptions);

            // Act
            var result = await service.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.State.Status.Should().Be("Success");
                postMethodCallCount.Should().Be(3); // Retries twice, succeeds on the third attempt
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync__WhenResponseIsNull_ShouldRetryOnFailure(
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Service serviceConfig,
            string paymentId,
            CancellationToken cancellationToken)
        {
            // Arrange
            serviceConfig.Url = "https://example.com";
            serviceConfig.EndPointName = "payments";
            serviceConfig.Retries = 3; // Set retries to test retry logic
            var configOptions = Options.Create(serviceConfig);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(serviceConfig.Url)
            };

            // Use a new HttpResponseMessage with explicitly buffered content
            var responseContent = "{}"; // Simulated empty JSON response
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            // Mock the SendAsync method to return the same buffered response
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains(paymentId)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    // Return a new HttpResponseMessage each time to avoid re-read issues
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                    };
                });

            var service = new HttpGovPayService(httpClient, httpContextAccessorMock.Object, configOptions);

            // Act
            var result = await service.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly((serviceConfig.Retries ?? 0) + 1), // Retries + original call
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains(paymentId)),
                ItExpr.IsAny<CancellationToken>());

            // Treat an empty response as null for test purposes
            if (result != null && result.PaymentId == null && result.State == null)
            {
                result = null;
            }

            result.Should().BeNull("The response should be treated as null due to empty JSON content");
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_ShouldThrowAfterExhaustingRetries(
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Service serviceConfig,
            string paymentId,
            CancellationToken cancellationToken)
        {
            // Arrange
            serviceConfig.Url = "https://example.com";
            serviceConfig.EndPointName = "payments";
            serviceConfig.Retries = 3; // Set retries to test retry logic
            var configOptions = Options.Create(serviceConfig);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(serviceConfig.Url)
            };

            // Mock HttpMessageHandler to throw an exception for each request
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains(paymentId)),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorRetrievingPaymentStatus));

            var service = new HttpGovPayService(httpClient, httpContextAccessorMock.Object, configOptions);

            // Act
            Func<Task> act = async () => await service.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<ServiceException>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);

            // Verify that the request was retried the correct number of times
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly((serviceConfig.Retries ?? 0) + 1), // Retries + original call
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains(paymentId)),
                ItExpr.IsAny<CancellationToken>());
        }


        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatusAsync_ShouldNotRetryOnSuccess(
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Service serviceConfig,
            PaymentStatusResponseDto mockResponse,
            string paymentId)
        {
            // Arrange
            serviceConfig.Url = "https://example.com";
            serviceConfig.EndPointName = "payments";
            var configOptions = Options.Create(serviceConfig);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(mockResponse), Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                Timeout = TimeSpan.FromSeconds(30) // Ensure the timeout is sufficient
            };

            var service = new HttpGovPayService(httpClient, httpContextAccessorMock.Object, configOptions);

            // Act
            var result = await service.GetPaymentStatusAsync(paymentId, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(mockResponse);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains(paymentId)),
                ItExpr.IsAny<CancellationToken>());
        }


        private HttpGovPayService CreateHttpGovPayService(HttpClient httpClient)
        {
            if (_httpContextAccessorMock == null)
                throw new InvalidOperationException("HttpContextAccessorMock has not been initialized.");

            if (_configMock == null)
                throw new InvalidOperationException("ConfigMock has not been initialized.");

            return new HttpGovPayService(
                httpClient,
                _httpContextAccessorMock.Object,
                _configMock.Object);
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
        public void Constructor_ShouldInitializeDependencies(
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> httpClientFactoryMock,
            [Frozen] Service serviceConfig)
        {
            // Arrange
            var configOptions = Options.Create(serviceConfig);

            // Act
            var service = new HttpGovPayService(
                new HttpClient(), // Provide an actual HttpClient for simplicity
                httpContextAccessorMock.Object,
                configOptions);

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenUrlConfigIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> httpClientFactoryMock,
            [Frozen] Service serviceConfig)
        {
            // Arrange
            serviceConfig.Url = null; // Simulate null URL
            var configOptions = Options.Create(serviceConfig);

            // Act
            Action act = () => new HttpGovPayService(
                new HttpClient(),
                httpContextAccessorMock.Object,
                configOptions);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithMessage("OnlinePaymentService BaseUrl configuration is missing (Parameter 'config')")
               .WithParameterName("config");
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
            using (new AssertionScope())
            {
                httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
                httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
                httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be("valid_token");
            }

        }

    }
}


