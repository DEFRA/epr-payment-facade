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
        private Mock<IHttpAccreditationFeesCalculatorService> _httpMock = null!;
        private Mock<ILogger<AccreditationFeesCalculatorService>> _loggerMock = null!;
        private AccreditationFeesCalculatorService _service = null!;

        static ReprocessorOrExporterAccreditationFeesRequestDto SampleRequest() => new()
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
            _httpMock   = new Mock<IHttpAccreditationFeesCalculatorService>();
            _loggerMock = new Mock<ILogger<AccreditationFeesCalculatorService>>();
            _service    = new AccreditationFeesCalculatorService(_httpMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ShouldReturnDto_WhenHttpServiceSucceeds()
        {
            // Arrange
            var request = SampleRequest();
            var expected = new ReprocessorOrExporterAccreditationFeesResponseDto
            {
                OverseasSiteChargePerSite = 5m,
                TotalOverseasSitesCharges = 15m,
                TonnageBandCharge         = 50m,
                TotalAccreditationFees    = 65m
            };
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            ReprocessorOrExporterAccreditationFeesResponseDto? actual
                = await _service.CalculateAccreditationFeesAsync(request, CancellationToken.None);

            // Assert
            Assert.AreSame(expected, actual);

            // Verify
            _httpMock.Verify(x => x.CalculateAccreditationFeesAsync(request, CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ShouldReturnNull_WhenHttpServiceReturnsNull()
        {
            // Arrange
            var request = SampleRequest();
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReprocessorOrExporterAccreditationFeesResponseDto?)null);

            // Act
            ReprocessorOrExporterAccreditationFeesResponseDto? actual
                = await _service.CalculateAccreditationFeesAsync(request, CancellationToken.None);

            // Assert
            Assert.IsNull(actual);

            // Verify
            _httpMock.Verify(x => x.CalculateAccreditationFeesAsync(request, CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ShouldThrowValidationException_OnHttpValidationException()
        {
            // Arrange
            var request = SampleRequest();
            var message = "Invalid payload";
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(message));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ValidationException>(
                () => _service.CalculateAccreditationFeesAsync(request, CancellationToken.None));

            // Verify
            _httpMock.Verify(x => x.CalculateAccreditationFeesAsync(request, CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ShouldThrowServiceException_OnGenericException()
        {
            // Arrange
            var request = SampleRequest();
            var inner = new InvalidOperationException("HTTP failure");
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(inner);

            // Act & Assert
            ServiceException ex = await Assert.ThrowsExceptionAsync<ServiceException>(
                () => _service.CalculateAccreditationFeesAsync(request, CancellationToken.None));
            Assert.AreEqual(ExceptionMessages.ErrorCalculatingAccreditationFees, ex.Message);
            Assert.AreSame(inner, ex.InnerException);

            // Verify
            _httpMock.Verify(x => x.CalculateAccreditationFeesAsync(request, CancellationToken.None), Times.Once);
        }
    }
}
