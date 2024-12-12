using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme;
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

namespace EPR.Payment.Facade.Common.UnitTests.RestServices
{
    [TestClass]
    public class HttpComplianceSchemeFeesServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IOptionsMonitor<Service>> _configMonitorMock = null!;
        private ComplianceSchemeFeesRequestDto _complianceSchemeFeesRequestDto = null!;
        private ComplianceSchemeFeesResponseDto _complianceSchemeFeesResponseDto = null!;

        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "compliance-scheme",
                HttpClientName = "HttpClientName"
            };

            _configMonitorMock = new Mock<IOptionsMonitor<Service>>();
            _configMonitorMock.Setup(x => x.Get("ComplianceSchemeFeesService")).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _complianceSchemeFeesRequestDto = new ComplianceSchemeFeesRequestDto
            {
                Regulator = "GB-ENG",
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                ComplianceSchemeMembers = new List<ComplianceSchemeMemberDto>
                {
                    new ComplianceSchemeMemberDto
                    {
                        MemberId = "123",
                        MemberType = "LARGE",
                        IsOnlineMarketplace = true,
                        IsLateFeeApplicable = true,
                        NumberOfSubsidiaries = 150,
                        NoOfSubsidiariesOnlineMarketplace = 10

                    }
                }
            };

            _complianceSchemeFeesResponseDto = new ComplianceSchemeFeesResponseDto
            {
                TotalFee = 6619100,
                ComplianceSchemeRegistrationFee = 1380400,
                PreviousPayment = 0,
                OutstandingPayment = 6619100,
                ComplianceSchemeMembersWithFees = new List<ComplianceSchemeMembersWithFeesDto>
                {
                    new ComplianceSchemeMembersWithFeesDto
                    {
                        MemberId = "123",
                        MemberRegistrationFee = 165800,
                        MemberOnlineMarketPlaceFee = 257900,
                        MemberLateRegistrationFee = 33200,
                        SubsidiariesFee = 4815000,
                        TotalMemberFee = 5238700,
                        SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdown
                        {
                                TotalSubsidiariesOMPFees = 2579000,
                                CountOfOMPSubsidiaries = 10,
                                UnitOMPFees = 257900,
                                FeeBreakdowns = new List<FeeBreakdown>
                                {
                                    new FeeBreakdown
                                    {
                                        BandNumber = 1,
                                        UnitCount = 20,
                                        UnitPrice = 55800,
                                        TotalPrice = 1116000
                                    },
                                    new FeeBreakdown
                                    {
                                        BandNumber = 2,
                                        UnitCount = 80,
                                        UnitPrice = 14000,
                                        TotalPrice = 1120000
                                    },
                                    new FeeBreakdown
                                    {
                                        BandNumber = 3,
                                        UnitCount = 50,
                                        UnitPrice = 0,
                                        TotalPrice = 0
                                    }
                                }
                        }
                    }
                }
            };
        }

        private HttpComplianceSchemeFeesService CreateHttpComplianceSchemeFeesService(HttpClient httpClient)
        {
            return new HttpComplianceSchemeFeesService(
                httpClient,
                _httpContextAccessorMock!.Object,
                _configMonitorMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_ConfigMonitorIsNull_ShouldThrowArgumentNullException(
            Mock<IHttpContextAccessor> httpContextAccessorMock,
            HttpClient httpClient)
        {
            // Act
            Action act = () => new HttpComplianceSchemeFeesService(httpClient, httpContextAccessorMock.Object, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("configMonitor");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ValidRequest_ReturnsRegistrationFeesResponseDto(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Mock<IOptions<Service>> configMock,
            HttpComplianceSchemeFeesService httpComplianceSchemeFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(_complianceSchemeFeesResponseDto), Encoding.UTF8, "application/json")
            });

            var httpClient = new HttpClient(handlerMock.Object);
            httpComplianceSchemeFeesService = CreateHttpComplianceSchemeFeesService(httpClient);

            // Act
            var result = await httpComplianceSchemeFeesService.CalculateFeesAsync(_complianceSchemeFeesRequestDto, cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(_complianceSchemeFeesResponseDto);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_HttpRequestException_ThrowsServiceException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Mock<IOptions<Service>> configMock,
            HttpComplianceSchemeFeesService httpComplianceSchemeFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException("Unexpected error"));

            var httpClient = new HttpClient(handlerMock.Object);
            httpComplianceSchemeFeesService = CreateHttpComplianceSchemeFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpComplianceSchemeFeesService.CalculateFeesAsync(_complianceSchemeFeesRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                    .WithMessage(ExceptionMessages.ErrorCalculatingComplianceSchemeFees);


                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_UnsuccessfulStatusCode_ThrowsValidationException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Mock<IOptions<Service>> configMock,
            HttpComplianceSchemeFeesService httpComplianceSchemeFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new ResponseCodeException(HttpStatusCode.BadRequest, "Invalid input parameter."));

            var httpClient = new HttpClient(handlerMock.Object);
            httpComplianceSchemeFeesService = CreateHttpComplianceSchemeFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpComplianceSchemeFeesService.CalculateFeesAsync(_complianceSchemeFeesRequestDto, cancellationToken);

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
        public async Task CalculateFeesAsync_Exception_ThrowsException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            Mock<IOptions<Service>> configMock,
            HttpComplianceSchemeFeesService httpComplianceSchemeFeesService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new Exception("Unexpected error."));

            var httpClient = new HttpClient(handlerMock.Object);
            httpComplianceSchemeFeesService = CreateHttpComplianceSchemeFeesService(httpClient);

            // Act
            Func<Task> act = async () => await httpComplianceSchemeFeesService.CalculateFeesAsync(_complianceSchemeFeesRequestDto, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<ServiceException>()
                .WithMessage("An unexpected error occurred while calculating Compliance Scheme fees.");

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
