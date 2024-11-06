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
    public class HttpComplianceSchemeResubmissionFeesServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptions<Service>> _configMock = null!;
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

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

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
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpContextAccessorIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IHttpClientFactory> httpClientFactoryMock,
            [Frozen] Mock<IOptions<Service>> configMock)
        {
            // Act
            Action act = () => new HttpComplianceSchemeResubmissionFeesService(null!, httpClientFactoryMock.Object, configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpContextAccessor");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpClientFactoryIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<IOptions<Service>> configMock)
        {
            // Act
            Action act = () => new HttpComplianceSchemeResubmissionFeesService(httpContextAccessorMock.Object, null!, configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClientFactory");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_ConfigUrlIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> httpClientFactoryMock)
        {
            // Arrange
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(c => c.Value).Returns(new Service { Url = null, EndPointName = "SomeEndPoint" });

            // Act
            Action act = () => new HttpComplianceSchemeResubmissionFeesService(httpContextAccessorMock.Object, httpClientFactoryMock.Object, configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.RegistrationFeesServiceBaseUrlMissing} (Parameter 'config')");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_ConfigEndPointNameIsNull_ShouldThrowArgumentNullException(
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock,
            [Frozen] Mock<IHttpClientFactory> httpClientFactoryMock)
        {
            // Arrange
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(c => c.Value).Returns(new Service { Url = "https://api.example.com", EndPointName = null });

            // Act
            Action act = () => new HttpComplianceSchemeResubmissionFeesService(httpContextAccessorMock.Object, httpClientFactoryMock.Object, configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.RegistrationFeesServiceEndPointNameMissing} (Parameter 'config')");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_ValidRequest_ReturnsComplianceSchemeResubmissionFeeResult(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            [Frozen] Mock<IOptions<Service>> configMock,
            HttpComplianceSchemeResubmissionFeesService httpComplianceSchemeResubmissionFeesService,
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
            [Frozen] Mock<IOptions<Service>> configMock,
            HttpComplianceSchemeResubmissionFeesService httpComplianceSchemeResubmissionFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
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
    }
}