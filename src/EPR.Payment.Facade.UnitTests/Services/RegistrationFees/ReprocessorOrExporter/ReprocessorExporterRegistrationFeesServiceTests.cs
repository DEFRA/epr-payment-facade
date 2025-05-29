using AutoFixture.AutoMoq;
using AutoFixture;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.Producer;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ReprocessorOrExporter.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.ReprocessorOrExporter;
using Microsoft.Extensions.Logging;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions.Execution;
using FluentAssertions;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;
using System.ComponentModel.DataAnnotations;

namespace EPR.Payment.Facade.UnitTests.Services.RegistrationFees.ReprocessorOrExporter
{
    [TestClass]
    public class ReprocessorExporterRegistrationFeesServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpReprocessorExporterRegistrationFeesService> _httpRepoExpoRegistrationFeesService = null!;
        private Mock<ILogger<ReprocessorExporterRegistrationFeesService>> _loggerMock = null!;
        private ReprocessorExporterRegistrationFeesService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _httpRepoExpoRegistrationFeesService = _fixture.Freeze<Mock<IHttpReprocessorExporterRegistrationFeesService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<ReprocessorExporterRegistrationFeesService>>>();

            _service = new ReprocessorExporterRegistrationFeesService(
                _httpRepoExpoRegistrationFeesService.Object,
                _loggerMock.Object);
        }
        [TestMethod, AutoMoqData]
        public void Constructor_HttpRegistrationFeesServiceIsNull_ShouldThrowArgumentNullException(
           ILogger<ReprocessorExporterRegistrationFeesService> logger)
        {
            // Act
            Action act = () => new ReprocessorExporterRegistrationFeesService(null!, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpReprocessorExporterRegistrationFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_LoggerIsNull_ShouldThrowArgumentNullException(
            IHttpReprocessorExporterRegistrationFeesService httpRepoExpoRegistrationFeesService)
        {
            // Act
            Action act = () => new ReprocessorExporterRegistrationFeesService(httpRepoExpoRegistrationFeesService, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateRepoExpoRegiFeesAsync_RequestIsNull_ShouldThrowArgumentNullException(
            ReprocessorExporterRegistrationFeesService service)
        {
            // Act
            Func<Task> act = () => service.CalculateFeesAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.ErroreproExpoRegServiceFee} (Parameter 'request')");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateRepoExpoRegiFeesAsync_RequestIsValid_ShouldReturnResponse(
            ReprocessorOrExporterRegistrationFeesResponseDto expectedResponse,
            ReprocessorOrExporterRegistrationFeesRequestDto request)
        {
            // Arrange
            _httpRepoExpoRegistrationFeesService.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.CalculateFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateRepoExpoRegiFeesAsync_HttpServiceThrowsException_ShouldLogAndThrowServiceException(
            ReprocessorOrExporterRegistrationFeesRequestDto request)
        {
            // Arrange
            var exceptionMessage = "Unexpected error occurred";
            var exception = new Exception(exceptionMessage);

            _httpRepoExpoRegistrationFeesService.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            Func<Task> act = async () => await _service.CalculateFeesAsync(request);

            // Assert
            var thrownException = await act.Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErroreproExpoRegServiceFee);

            thrownException.Which.InnerException.Should().BeOfType<Exception>()
                .Which.Message.Should().Be(exceptionMessage);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.UnexpectedErroreproExpoRegServiceFees)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateRepoExpoRegiFeesAsync_HttpServiceThrowsException_ShouldLogAndThrowValidationException(
            ReprocessorOrExporterRegistrationFeesRequestDto request)
        {
            // Arrange
            var exceptionMessage = "Validation error";
            var validationException = new ValidationException(exceptionMessage);

            _httpRepoExpoRegistrationFeesService.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
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
