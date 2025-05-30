using System.Net;
using System.Text;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.AccreditationFees;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Payment.Facade.Common.UnitTests.RESTServices
{
    [TestClass]
    public class HttpAccreditationFeesCalculatorServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptionsMonitor<Service>> _configMonitorMock = null!;
        private ReprocessorOrExporterAccreditationFeesRequestDto _accreditationFeesRequestDto = null!;
        private ReprocessorOrExporterAccreditationFeesResponseDto _accreditationFeesResponseDto = null!;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://api.example.com",
                EndPointName = "accreditation-fee",
                HttpClientName = "HttpClientName"
            };

            _configMonitorMock = new Mock<IOptionsMonitor<Service>>();
            _configMonitorMock.Setup(x => x.Get("RexExpoAccreditationFeesService")).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _accreditationFeesRequestDto = new()
            {
                RequestorType = RequestorTypes.Exporters,
                Regulator = "GN-ENG",
                TonnageBand = TonnageBands.Upto500,
                NumberOfOverseasSites = 10,
                MaterialType = MaterialTypes.Plastic,
                ApplicationReferenceNumber = "REF123",
                SubmissionDate = DateTime.UtcNow
            };

            _accreditationFeesResponseDto = new()
            {
                OverseasSiteChargePerSite = 10,
                TonnageBandCharge = 1000,
                TotalOverseasSitesCharges = 100,
                TotalAccreditationFees = 1100,
                PreviousPaymentDetail = new()
                {
                    PaymentMode = "Offline",
                    PaymentMethod = "Bank Transfer",
                    PaymentAmount = 1100,
                    PaymentDate = DateTime.UtcNow.AddDays(-1)
                },
            };
        }

        private HttpAccreditationFeesCalculatorService CreateHttpAccreditationFeesCalculatorService(HttpClient httpClient)
        {
            return new HttpAccreditationFeesCalculatorService(
                httpClient,
                _httpContextAccessorMock!.Object,
                _configMonitorMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpContextAccessorIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new HttpAccreditationFeesCalculatorService(
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
            Action act = () => new HttpAccreditationFeesCalculatorService(httpClient, httpContextAccessor, null!);

            // Assert that the exception is of type NullReferenceException
            act.Should().Throw<NullReferenceException>();
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_ValidRequest_ReturnsRegistrationFeesResponseDto(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpAccreditationFeesCalculatorService httpAccreditationFeesCalculatorService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_accreditationFeesResponseDto), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpAccreditationFeesCalculatorService = CreateHttpAccreditationFeesCalculatorService(httpClient);

            // Act
            ReprocessorOrExporterAccreditationFeesResponseDto? result = await httpAccreditationFeesCalculatorService.CalculateAccreditationFeesAsync(_accreditationFeesRequestDto, cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(_accreditationFeesResponseDto);
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
            HttpAccreditationFeesCalculatorService httpAccreditationFeesCalculatorService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Unexpected error"));

            var httpClient = new HttpClient(handlerMock.Object);
            httpAccreditationFeesCalculatorService = CreateHttpAccreditationFeesCalculatorService(httpClient);

            // Act
            Func<Task> act = async () => await httpAccreditationFeesCalculatorService.CalculateAccreditationFeesAsync(_accreditationFeesRequestDto, cancellationToken);

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
            HttpAccreditationFeesCalculatorService httpAccreditationFeesCalculatorService,
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
            httpAccreditationFeesCalculatorService = CreateHttpAccreditationFeesCalculatorService(httpClient);

            // Act
            Func<Task> act = async () =>
            {
               _ = await httpAccreditationFeesCalculatorService.CalculateAccreditationFeesAsync(_accreditationFeesRequestDto, cancellationToken) ?? throw new ServiceException(ExceptionMessages.ErrorCalculatingProducerFees);
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
            HttpAccreditationFeesCalculatorService httpAccreditationFeesCalculatorService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new ResponseCodeException(HttpStatusCode.BadRequest, "Invalid input parameter."));

            var httpClient = new HttpClient(handlerMock.Object);
            httpAccreditationFeesCalculatorService = CreateHttpAccreditationFeesCalculatorService(httpClient);

            // Act
            Func<Task> act = async () => await httpAccreditationFeesCalculatorService.CalculateAccreditationFeesAsync(_accreditationFeesRequestDto, cancellationToken);

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

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_UnsuccessfulNotFoundStatusCode_ThrowsValidationException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            HttpAccreditationFeesCalculatorService httpAccreditationFeesCalculatorService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new ResponseCodeException(HttpStatusCode.NotFound, "Not Found."));

            var httpClient = new HttpClient(handlerMock.Object);
            httpAccreditationFeesCalculatorService = CreateHttpAccreditationFeesCalculatorService(httpClient);

            // Act
            ReprocessorOrExporterAccreditationFeesResponseDto? result = await httpAccreditationFeesCalculatorService.CalculateAccreditationFeesAsync(_accreditationFeesRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeNull();

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