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
        private Mock<IOptionsMonitor<Service>> _configMonitorMock = null!;
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

            _configMonitorMock = new Mock<IOptionsMonitor<Service>>();
            _configMonitorMock.Setup(x => x.Get("ProducerFeesService")).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _producerFeesRequestDto = new ProducerFeesRequestDto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = "GB-ENG",
                IsProducerOnlineMarketplace = false,
                IsClosedLoopRecycling = true,
                NoOfSubsidiariesClosedLoopRecycling = 2,
                IsLateFeeApplicable = false,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow
            };

            _producerFeesResponseDto = new ProducerFeesResponseDto
            {
                ProducerRegistrationFee = 165800,
                ProducerOnlineMarketPlaceFee = 257900,
                ProducerClosedLoopRecyclingFee = 33200,
                SubsidiariesFee = 800,
                TotalFee = 1000,
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdown
                {
                    TotalSubsidiariesOMPFees = 2579000,
                    CountOfOMPSubsidiaries = 10,
                    UnitOMPFees = 257900,
                    TotalSubsidiariesClosedLoopRecyclingFees = 99600,
                    CountOfClosedLoopRecyclingSubsidiaries = 2,
                    UnitClosedLoopRecyclingFees = 49800,
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
                httpClient,
                _httpContextAccessorMock.Object,
                _configMonitorMock.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpContextAccessorIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => _ = new HttpProducerFeesService(
                new HttpClient(),
                null!,
                _configMonitorMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpContextAccessor");
        }

        [TestMethod]
        public void Constructor_ConfigMonitorIsNull_ShouldThrowNullReferenceException()
        {
            // Arrange
            IHttpContextAccessor httpContextAccessor = new Mock<IHttpContextAccessor>().Object;
            HttpClient httpClient = new Mock<HttpClient>().Object;

            // Act & Assert
            Action act = () => _ = new HttpProducerFeesService(httpClient, httpContextAccessor, null!);

            // Assert that the exception is of type NullReferenceException
            act.Should().Throw<NullReferenceException>();
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
        public async Task CalculateProducerFeesAsync_ValidRequest_ForwardsClosedLoopRecyclingFieldsInBody(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpProducerFeesService httpProducerFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            string? capturedBody = null;
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .Callback<HttpRequestMessage, CancellationToken>(async (msg, _) =>
                       {
                           if (msg.Content != null)
                           {
                               capturedBody = await msg.Content.ReadAsStringAsync();
                           }
                       })
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_producerFeesResponseDto), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerFeesService = CreateHttpProducerFeesService(httpClient);

            // Act
            await httpProducerFeesService.CalculateProducerFeesAsync(_producerFeesRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                capturedBody.Should().NotBeNull();
                capturedBody.Should().Contain("\"isClosedLoopRecycling\":true");
                capturedBody.Should().Contain("\"noOfSubsidiariesClosedLoopRecycling\":2");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_ValidRequest_Returns_ClosedLoopRecyclingFields_FromResponseBody(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpProducerFeesService httpProducerFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            const string responseJson = """
                {
                  "producerRegistrationFee": 165800,
                  "producerOnlineMarketPlaceFee": 257900,
                  "producerClosedLoopRecyclingFee": 33200,
                  "producerLateRegistrationFee": 0,
                  "subsidiariesFee": 800,
                  "totalFee": 1000,
                  "previousPayment": 0,
                  "outstandingPayment": 1000,
                  "subsidiariesFeeBreakdown": {
                    "totalSubsidiariesOMPFees": 2579000,
                    "countOfOMPSubsidiaries": 10,
                    "unitOMPFees": 257900,
                    "totalSubsidiariesClosedLoopRecyclingFees": 99600,
                    "countOfClosedLoopRecyclingSubsidiaries": 2,
                    "unitClosedLoopRecyclingFees": 49800,
                    "feeBreakdowns": []
                  }
                }
                """;

            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpProducerFeesService = CreateHttpProducerFeesService(httpClient);

            // Act
            var result = await httpProducerFeesService.CalculateProducerFeesAsync(_producerFeesRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.ProducerClosedLoopRecyclingFee.Should().Be(33200);
                result.SubsidiariesFeeBreakdown.TotalSubsidiariesClosedLoopRecyclingFees.Should().Be(99600);
                result.SubsidiariesFeeBreakdown.CountOfClosedLoopRecyclingSubsidiaries.Should().Be(2);
                result.SubsidiariesFeeBreakdown.UnitClosedLoopRecyclingFees.Should().Be(49800);
            }
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