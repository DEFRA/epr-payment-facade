using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.ResubmissionFees.ComplianceScheme
{
    [TestClass]
    public class ComplianceSchemeResubmissionFeesServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpComplianceSchemeResubmissionFeesService> _httpComplianceSchemeResubmissionFeesService = null!;
        private Mock<ILogger<ComplianceSchemeResubmissionFeesService>> _loggerMock = null!;
        private ComplianceSchemeResubmissionFeesService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _httpComplianceSchemeResubmissionFeesService = _fixture.Freeze<Mock<IHttpComplianceSchemeResubmissionFeesService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<ComplianceSchemeResubmissionFeesService>>>();

            _service = new ComplianceSchemeResubmissionFeesService(
                _httpComplianceSchemeResubmissionFeesService.Object,
                _loggerMock.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpResubmissionFeesServiceIsNull_ShouldThrowArgumentNullException(
            ILogger<ComplianceSchemeResubmissionFeesService> logger)
        {
            // Act
            Action act = () => new ComplianceSchemeResubmissionFeesService(null!, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpComplianceSchemeResubmissionFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_LoggerIsNull_ShouldThrowArgumentNullException(
            IHttpComplianceSchemeResubmissionFeesService httpComplianceSchemeResubmissionFeesService)
        {
            // Act
            Action act = () => new ComplianceSchemeResubmissionFeesService(httpComplianceSchemeResubmissionFeesService, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_RequestIsNull_ShouldThrowArgumentNullException(
            ComplianceSchemeResubmissionFeesService service)
        {
            // Act
            Func<Task> act = () => service.CalculateResubmissionFeeAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.ErrorResubmissionFees} (Parameter 'request')");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_RequestIsValid_ShouldReturnResponse(
            ComplianceSchemeResubmissionFeeResult expectedResponse,
            ComplianceSchemeResubmissionFeeRequestDto request)
        {
            // Arrange
            _httpComplianceSchemeResubmissionFeesService.Setup(s => s.CalculateResubmissionFeeAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.CalculateResubmissionFeeAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_HttpServiceThrowsException_ShouldLogAndThrowServiceException(
            ComplianceSchemeResubmissionFeeRequestDto request)
        {
            // Arrange
            var exceptionMessage = "Unexpected error occurred";
            var exception = new Exception(exceptionMessage);

            _httpComplianceSchemeResubmissionFeesService.Setup(s => s.CalculateResubmissionFeeAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            Func<Task> act = async () => await _service.CalculateResubmissionFeeAsync(request);

            // Assert
            var thrownException = await act.Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorResubmissionFees);

            thrownException.Which.InnerException.Should().BeOfType<Exception>()
                .Which.Message.Should().Be(exceptionMessage);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.ErrorResubmissionFees)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}