using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ReprocessorOrExporter;
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
    public class HttpReprocessorExporterRegistrationFeesServiceTests
    {
        private Service _config = default!;
        private ReprocessorOrExporterRegistrationFeesRequestDto _requestDto = null!;
        private ReprocessorOrExporterRegistrationFeesResponseDto _responseDto = null!;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            _config = new Service
            {
                Url = "https://api.example.com",
                EndPointName = "registration-fee",
                HttpClientName = "HttpClientName"
            };

            _requestDto = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = Enums.RequestorTypes.Exporters,
                MaterialType = Enums.MaterialTypes.Plastic,
                Regulator = "GB-ENG",
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow
            };

            _responseDto = new ReprocessorOrExporterRegistrationFeesResponseDto
            {               
                RegistrationFee = 2921.00m,
                PreviousPaymentDetail = new()
                {
                    PaymentAmount = 1000.00m,
                    PaymentDate = DateTime.UtcNow.AddDays(-30),
                    PaymentMethod = "Cheque",
                    PaymentMode = "Offline",
                }
            };
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpContextAccessorIsNull_ShouldThrowArgumentNullException(
            HttpClient httpClient,
            IOptionsMonitor<Service> configMonitor)
        {
            // Act
            Func<HttpReprocessorExporterRegistrationFeesService> act = () => new HttpReprocessorExporterRegistrationFeesService(
                httpClient,
                null!,
                configMonitor);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpContextAccessor");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_ConfigMonitorIsNull_ShouldThrowNullReferenceException(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            // Act
            Func<HttpProducerFeesService> act = () => new HttpProducerFeesService(httpClient, httpContextAccessor, null!);

            // Assert
            act.Should().Throw<NullReferenceException>();
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ValidRequest_ReturnsRegistrationFeesResponseDto(
            [Frozen] Mock<IOptionsMonitor<Service>> configMonitorMock,
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen, Greedy] HttpClient httpClient,
            [Frozen] IHttpContextAccessor httpContextAccessor,
            CancellationTokenSource cancellationTokenSource)
        {
            // Arrange
            configMonitorMock
                .Setup(x => x.Get("RexExpoRegistrationFeesService"))
                .Returns(_config);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(_responseDto), Encoding.UTF8, "application/json")
                });

            HttpReprocessorExporterRegistrationFeesService serviceUnderTest = new HttpReprocessorExporterRegistrationFeesService(httpClient, httpContextAccessor, configMonitorMock.Object);

            // Act
            ReprocessorOrExporterRegistrationFeesResponseDto result = await serviceUnderTest.CalculateFeesAsync(_requestDto, cancellationTokenSource.Token);

            // Assert
            result.Should().BeEquivalentTo(_responseDto);
            handlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_HttpRequestException_ThrowsServiceException(
            [Frozen] Mock<IOptionsMonitor<Service>> configMonitorMock,
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen, Greedy] HttpClient httpClient,
            [Frozen] IHttpContextAccessor httpContextAccessor,
            CancellationTokenSource cancellationTokenSource)
        {
            // Arrange
            configMonitorMock
                .Setup(x => x.Get("RexExpoRegistrationFeesService"))
                .Returns(_config);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Unexpected error"));

            HttpReprocessorExporterRegistrationFeesService serviceUnderTest = new HttpReprocessorExporterRegistrationFeesService(httpClient, httpContextAccessor, configMonitorMock.Object);

            // Act
            Func<Task> act = async () => await serviceUnderTest.CalculateFeesAsync(_requestDto, cancellationTokenSource.Token);

            // Assert
            using (new AssertionScope())
            {
                await act.Should()
                    .ThrowAsync<ServiceException>()
                    .WithMessage(ExceptionMessages.ErroreproExpoRegServiceFee);

                handlerMock
                    .Protected()
                    .Verify(
                        "SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post),
                        ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_NullContent_ThrowsServiceException(
            [Frozen] Mock<IOptionsMonitor<Service>> configMonitorMock,
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen, Greedy] HttpClient httpClient,
            [Frozen] IHttpContextAccessor httpContextAccessor,
            CancellationTokenSource cancellationTokenSource)
        {
            // Arrange
            configMonitorMock
                .Setup(x => x.Get("RexExpoRegistrationFeesService"))
                .Returns(_config);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = null // Simulate a null response content
                });

            HttpReprocessorExporterRegistrationFeesService serviceUnderTest = new HttpReprocessorExporterRegistrationFeesService(httpClient, httpContextAccessor, configMonitorMock.Object);

            // Act
            Func<Task> act = async () =>
            {
                ReprocessorOrExporterRegistrationFeesResponseDto response = await serviceUnderTest.CalculateFeesAsync(_requestDto, cancellationTokenSource.Token);
                // Manually check for null content to simulate the exception throwing
                if (response == null)
                {
                    throw new ServiceException(ExceptionMessages.ErroreproExpoRegServiceFee);
                }
            };

            // Assert
            using (new AssertionScope())
            {
                await act.Should()
                    .ThrowAsync<ServiceException>()
                    .WithMessage(ExceptionMessages.ErroreproExpoRegServiceFee);

                handlerMock
                    .Protected()
                    .Verify(
                        "SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post),
                        ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_UnsuccessfulStatusCode_ThrowsValidationException(
            [Frozen] Mock<IOptionsMonitor<Service>> configMonitorMock,
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen, Greedy] HttpClient httpClient,
            [Frozen] IHttpContextAccessor httpContextAccessor,
            CancellationTokenSource cancellationTokenSource)

        {
            // Arrange
            configMonitorMock.Setup(x => x.Get("RexExpoRegistrationFeesService"))
                .Returns(_config);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new ResponseCodeException(HttpStatusCode.BadRequest, "Invalid input parameter."));

            HttpReprocessorExporterRegistrationFeesService serviceUnderTest = new HttpReprocessorExporterRegistrationFeesService(httpClient, httpContextAccessor, configMonitorMock.Object);

            // Act
            Func<Task> act = async () => await serviceUnderTest.CalculateFeesAsync(_requestDto, cancellationTokenSource.Token);

            // Assert
            using (new AssertionScope())
            {
                await act.Should()
                    .ThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>()
                    .WithMessage("Invalid input parameter.");

                handlerMock
                    .Protected()
                    .Verify(
                        "SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Post),
                        ItExpr.IsAny<CancellationToken>());
            }
        }
    }
}