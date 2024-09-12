using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.RegistrationFees.ComplianceScheme
{
    [TestClass]
    public class ComplianceSchemeFeesServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpComplianceSchemeFeesService> _httpComplianceSchemeFeesServiceMock = null!;
        private Mock<ILogger<ComplianceSchemeFeesService>> _loggerMock = null!;
        private ComplianceSchemeFeesService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _httpComplianceSchemeFeesServiceMock = _fixture.Freeze<Mock<IHttpComplianceSchemeFeesService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<ComplianceSchemeFeesService>>>();

            _service = new ComplianceSchemeFeesService(
                _httpComplianceSchemeFeesServiceMock.Object,
                _loggerMock.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpComplianceSchemeFeesServiceIsNull_ShouldThrowArgumentNullException(
            ILogger<ComplianceSchemeFeesService> logger)
        {
            // Act
            Action act = () => new ComplianceSchemeFeesService(null!, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpComplianceSchemeFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_LoggerIsNull_ShouldThrowArgumentNullException(
            IHttpComplianceSchemeFeesService httpComplianceSchemeFeesService)
        {
            // Act
            Action act = () => new ComplianceSchemeFeesService(httpComplianceSchemeFeesService, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task GetComplianceSchemeBaseFeeAsync_RequestIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Func<Task> act = () => _service.GetComplianceSchemeBaseFeeAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee} (Parameter 'request')");
        }

        [TestMethod, AutoMoqData]
        public async Task GetComplianceSchemeBaseFeeAsync_RequestIsValid_ShouldReturnResponse(
            ComplianceSchemeBaseFeeResponse expectedResponse,
            RegulatorDto request)
        {
            // Arrange
            _httpComplianceSchemeFeesServiceMock.Setup(s => s.GetComplianceSchemeBaseFeeAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.GetComplianceSchemeBaseFeeAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod, AutoMoqData]
        public async Task GetComplianceSchemeBaseFeeAsync_InvalidBaseFee_ShouldThrowServiceException(
            RegulatorDto request)
        {
            // Arrange
            var invalidResponse = new ComplianceSchemeBaseFeeResponse { BaseFee = 0 };
            _httpComplianceSchemeFeesServiceMock.Setup(s => s.GetComplianceSchemeBaseFeeAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invalidResponse);

            // Act
            Func<Task> act = async () => await _service.GetComplianceSchemeBaseFeeAsync(request);

            // Assert
            await act.Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee); // Expect the general message

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid base fee returned for regulator")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
                Times.Once);
        }

        [TestMethod, AutoMoqData]
        public async Task GetComplianceSchemeBaseFeeAsync_HttpServiceThrowsException_ShouldLogAndThrowServiceException(
            RegulatorDto request)
        {
            // Arrange
            var exceptionMessage = "Unexpected error occurred";
            var exception = new Exception(exceptionMessage);

            _httpComplianceSchemeFeesServiceMock.Setup(s => s.GetComplianceSchemeBaseFeeAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            Func<Task> act = async () => await _service.GetComplianceSchemeBaseFeeAsync(request);

            // Assert
            var thrownException = await act.Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee);

            thrownException.Which.InnerException.Should().BeOfType<Exception>()
                .Which.Message.Should().Be(exceptionMessage);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.UnexpectedErrorRetrievingComplianceSchemeBaseFee)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
                Times.Once);
        }
    }
}
