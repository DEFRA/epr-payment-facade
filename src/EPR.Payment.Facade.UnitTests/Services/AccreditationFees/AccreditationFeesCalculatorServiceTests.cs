using System;
using System.ComponentModel.DataAnnotations;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.AccreditationFees.Interfaces;
using EPR.Payment.Facade.Services.AccreditationFees;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.AccreditationFees
{
    [TestClass]
    public class AccreditationFeesCalculatorServiceTests
    {
        private Mock<IHttpAccreditationFeesCalculatorService> _httpMock = new();
        private Mock<ILogger<AccreditationFeesCalculatorService>> _loggerMock = new();
        private AccreditationFeesCalculatorService? _accreditationFeesCalculatorServiceUnderTest;
        private CancellationToken _cancellationToken;

        private static ReprocessorOrExporterAccreditationFeesRequestDto AccreditationFeesRequestDto() => new()
        {
            RequestorType              = RequestorTypes.Exporters,
            Regulator                  = "RG-TST",
            TonnageBand                = TonnageBands.Upto500,
            NumberOfOverseasSites      = 3,
            MaterialType               = MaterialTypes.Plastic,
            ApplicationReferenceNumber = "APP-001",
            SubmissionDate             = DateTime.UtcNow
        };

        [TestInitialize]
        public void Init()
        {
            _accreditationFeesCalculatorServiceUnderTest    = new AccreditationFeesCalculatorService(
                _httpMock.Object,
                _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ShouldReturnDto_WhenHttpServiceSucceeds()
        {
            // Arrange
            var request = AccreditationFeesRequestDto();
            var expected = new ReprocessorOrExporterAccreditationFeesResponseDto
            {
                OverseasSiteChargePerSite = 5m,
                TotalOverseasSitesCharges = 15m,
                TonnageBandCharge         = 50m,
                TotalAccreditationFees    = 65m
            };

            // Setup
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(request, _cancellationToken))
                .ReturnsAsync(expected);

            // Act
            ReprocessorOrExporterAccreditationFeesResponseDto? actual
                = await _accreditationFeesCalculatorServiceUnderTest!.CalculateAccreditationFeesAsync(request, _cancellationToken);

            // Assert
            Assert.AreSame(expected, actual);

            // Verify
            _httpMock.Verify(x => x.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Once());
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ShouldReturnNull_WhenHttpServiceReturnsNull()
        {
            // Arrange
            var request = AccreditationFeesRequestDto();
            
            // Setup
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(request, _cancellationToken))
                .ReturnsAsync((ReprocessorOrExporterAccreditationFeesResponseDto?)null);

            // Act
            ReprocessorOrExporterAccreditationFeesResponseDto? actual
                = await _accreditationFeesCalculatorServiceUnderTest!.CalculateAccreditationFeesAsync(request, _cancellationToken);

            // Assert
            Assert.IsNull(actual);

            // Verify
            _httpMock.Verify(x => x.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Once());
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ShouldThrowValidationException_OnHttpValidationException()
        {
            // Arrange
            var request = AccreditationFeesRequestDto();
            var message = "Invalid payload";
            
            // Setup
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(request, _cancellationToken))
                .ThrowsAsync(new ValidationException(message));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _accreditationFeesCalculatorServiceUnderTest!.CalculateAccreditationFeesAsync(request, _cancellationToken));

            // Verify
            _httpMock.Verify(x => x.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Once());
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ShouldThrowServiceException_OnGenericException()
        {
            // Arrange
            var request = AccreditationFeesRequestDto();
            var inner = new InvalidOperationException("HTTP failure");
            
            // Setup
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(request, _cancellationToken))
                .ThrowsAsync(inner);

            // Act & Assert
            ServiceException ex = await Assert.ThrowsExceptionAsync<ServiceException>(
                () => _accreditationFeesCalculatorServiceUnderTest!.CalculateAccreditationFeesAsync(request, _cancellationToken));
            Assert.AreEqual(ExceptionMessages.ErrorCalculatingAccreditationFees, ex.Message);
            Assert.AreSame(inner, ex.InnerException);

            // Verify
            _httpMock.Verify(x => x.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Once());
        }
    }
}
