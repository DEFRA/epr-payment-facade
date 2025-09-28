using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.RegistrationFees.ComplianceScheme
{
    [TestClass]
    public class ComplianceSchemeFeesServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpComplianceSchemeFeesService> _httpComplianceSchemeFeesServiceMock = null!;
        private Mock<ILogger<ComplianceSchemeCalculatorService>> _loggerMock = null!;
        private ComplianceSchemeCalculatorService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _httpComplianceSchemeFeesServiceMock = _fixture.Freeze<Mock<IHttpComplianceSchemeFeesService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<ComplianceSchemeCalculatorService>>>();

            _service = new ComplianceSchemeCalculatorService(
                _httpComplianceSchemeFeesServiceMock.Object,
                _loggerMock.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpComplianceSchemeFeesServiceIsNull_ShouldThrowArgumentNullException(
            ILogger<ComplianceSchemeCalculatorService> logger)
        {
            // Act
            Action act = () => new ComplianceSchemeCalculatorService(null!, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpComplianceSchemeFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_LoggerIsNull_ShouldThrowArgumentNullException(
            IHttpComplianceSchemeFeesService httpComplianceSchemeFeesService)
        {
            // Act
            Action act = () => new ComplianceSchemeCalculatorService(httpComplianceSchemeFeesService, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_RequestIsValid_ShouldReturnResponse(
            ComplianceSchemeFeesResponseDto expectedResponse,
            ComplianceSchemeFeesRequestDto request)
        {
            // Arrange
            _httpComplianceSchemeFeesServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.CalculateFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_HttpServiceThrowsException_ShouldLogAndThrowServiceException(
            ComplianceSchemeFeesRequestDto request)
        {
            // Arrange
            var exceptionMessage = "Unexpected error occurred";
            var exception = new Exception(exceptionMessage);

            _httpComplianceSchemeFeesServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            Func<Task> act = async () => await _service.CalculateFeesAsync(request);

            // Assert
            using (new AssertionScope())
            {
                var thrownException = await act.Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorCalculatingComplianceSchemeFees);

                thrownException.Which.InnerException.Should().BeOfType<Exception>()
                    .Which.Message.Should().Be(exceptionMessage);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.UnexpectedErrorCalculatingComplianceSchemeFees)),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_HttpServiceThrowsException_ShouldLogAndThrowValidationException(
            ComplianceSchemeFeesRequestDto request)
        {
            // Arrange
            var exceptionMessage = "Validation error";
            var validationException = new ValidationException(exceptionMessage);

            _httpComplianceSchemeFeesServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act
            Func<Task> act = async () => await _service.CalculateFeesAsync(request);

            // Assert
            using (new AssertionScope())
            {
                var thrownException = await act.Should().ThrowAsync<ValidationException>();

                thrownException.Which.Message.Should().Be(exceptionMessage);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsyncV3_RequestIsValid_ShouldReturnResponse(
            ComplianceSchemeFeesResponseDto expectedResponse,
            ComplianceSchemeFeesRequestV3Dto request)
        {
            // Arrange
            _httpComplianceSchemeFeesServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.CalculateFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsyncV3_HttpServiceThrowsException_ShouldLogAndThrowServiceException(
            ComplianceSchemeFeesRequestV3Dto request)
        {
            // Arrange
            var exceptionMessage = "Unexpected error occurred";
            var exception = new Exception(exceptionMessage);

            _httpComplianceSchemeFeesServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            Func<Task> act = async () => await _service.CalculateFeesAsync(request);

            // Assert
            using (new AssertionScope())
            {
                var thrownException = await act.Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorCalculatingComplianceSchemeFees);

                thrownException.Which.InnerException.Should().BeOfType<Exception>()
                    .Which.Message.Should().Be(exceptionMessage);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.UnexpectedErrorCalculatingComplianceSchemeFees)),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsyncV3_HttpServiceThrowsException_ShouldLogAndThrowValidationException(
            ComplianceSchemeFeesRequestV3Dto request)
        {
            // Arrange
            var exceptionMessage = "Validation error";
            var validationException = new ValidationException(exceptionMessage);

            _httpComplianceSchemeFeesServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act
            Func<Task> act = async () => await _service.CalculateFeesAsync(request);

            // Assert
            using (new AssertionScope())
            {
                var thrownException = await act.Should().ThrowAsync<ValidationException>();

                thrownException.Which.Message.Should().Be(exceptionMessage);
            }
        }
    }
}
