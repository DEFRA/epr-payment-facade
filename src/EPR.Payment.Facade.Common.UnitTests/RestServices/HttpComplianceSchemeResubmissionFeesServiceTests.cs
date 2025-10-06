using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme;
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
    public class HttpComplianceSchemeResubmissionFeesServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptionsMonitor<Service>> _configMonitorMock = null!;
        private ComplianceSchemeResubmissionFeeRequestDto _requestDto = null!;
        private ComplianceSchemeResubmissionFeeResponse _responseDto = null!;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://api.example.com",
                EndPointName = "resubmission-fee",
                HttpClientName = "HttpClientName"
            };

            _configMonitorMock = new Mock<IOptionsMonitor<Service>>();
            // Return the mock config when Get("ComplianceSchemeFeesService") is called
            _configMonitorMock.Setup(x => x.Get("ComplianceSchemeFeesService")).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _requestDto = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow.AddDays(-1),
                ReferenceNumber = "CS-REF-1234",
                MemberCount = 5
            };

            _responseDto = new ComplianceSchemeResubmissionFeeResponse
            {
                TotalResubmissionFee = 215000,
                PreviousPayments = 50000,
                OutstandingPayment = 165000,
                MemberCount = 5
            };
        }

        private HttpComplianceSchemeResubmissionFeesService CreateHttpComplianceSchemeResubmissionFeesService(HttpClient httpClient)
        {
            return new HttpComplianceSchemeResubmissionFeesService(
                httpClient,
                _httpContextAccessorMock!.Object,
                _configMonitorMock!.Object);  // Ensure correct mock is passed
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_ValidRequest_ReturnsComplianceSchemeResubmissionFeeResult(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Mock<IOptionsMonitor<Service>> configMock,
            HttpComplianceSchemeResubmissionFeesService httpComplianceSchemeResubmissionFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            var config = new Service
            {
                Url = "https://api.example.com",
                EndPointName = "resubmission-fee",
                HttpClientName = "HttpClientName"
            };

            // Mocking IOptionsMonitor<Service> to return the configuration
            configMock.Setup(x => x.CurrentValue).Returns(config);

            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_responseDto), Encoding.UTF8, "application/json")
                       });

            var httpClient = new HttpClient(handlerMock.Object);
            httpComplianceSchemeResubmissionFeesService = CreateHttpComplianceSchemeResubmissionFeesService(httpClient);

            // Act
            var result = await httpComplianceSchemeResubmissionFeesService.CalculateResubmissionFeeAsync(_requestDto, cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(_responseDto);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_HttpRequestException_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Mock<IOptionsMonitor<Service>> configMock,
            HttpComplianceSchemeResubmissionFeesService httpComplianceSchemeResubmissionFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            var config = new Service
            {
                Url = "https://api.example.com",
                EndPointName = "resubmission-fee",
                HttpClientName = "HttpClientName"
            };

            // Mocking IOptionsMonitor<Service> to return the configuration
            configMock.Setup(x => x.CurrentValue).Returns(config);

            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Unexpected error"));

            var httpClient = new HttpClient(handlerMock.Object);
            httpComplianceSchemeResubmissionFeesService = CreateHttpComplianceSchemeResubmissionFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpComplianceSchemeResubmissionFeesService.CalculateResubmissionFeeAsync(_requestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                    .WithMessage(ExceptionMessages.ErrorResubmissionFees);

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_UnsuccessfulStatusCode_ThrowsValidationException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Mock<IOptionsMonitor<Service>> configMock,
            HttpComplianceSchemeResubmissionFeesService httpComplianceSchemeResubmissionFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new ResponseCodeException(HttpStatusCode.BadRequest, "Invalid input parameter."));

            var httpClient = new HttpClient(handlerMock.Object);
            httpComplianceSchemeResubmissionFeesService = CreateHttpComplianceSchemeResubmissionFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpComplianceSchemeResubmissionFeesService.CalculateResubmissionFeeAsync(_requestDto, cancellationToken);

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
