using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.AccreditationFees.Interfaces;
using EPR.Payment.Facade.Services.AccreditationFees;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.AccreditationFees
{
    [TestClass]
    public class AccreditationFeesCalculatorServiceTests
    {
        private Mock<IHttpAccreditationFeesCalculatorService> _httpMock;
        private Mock<ILogger<AccreditationFeesCalculatorService>> _logMock;
        private AccreditationFeesCalculatorService _service;

        [TestInitialize]
        public void Init()
        {
            _httpMock = new Mock<IHttpAccreditationFeesCalculatorService>();
            _logMock  = new Mock<ILogger<AccreditationFeesCalculatorService>>();
            _service  = new AccreditationFeesCalculatorService(_httpMock.Object, _logMock.Object);
        }

        private static AccreditationFeesRequestDto SampleRequest() => new AccreditationFeesRequestDto
        {
            RequestorType = RequestorTypes.Exporters,
            Regulator = "RG-001",
            TonnageBand = TonnageBands.Upto500,
            NumberOfOverseasSites = 2,
            MaterialType = MaterialTypes.Aluminium,
            ApplicationReferenceNumber = "APP-123",
            SubmissionDate = DateTime.UtcNow
        };

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ReturnsResponse_WhenHttpServiceSucceeds()
        {
            // Arrange
            var expected = new AccreditationFeesResponseDto
            {
                OverseasSiteChargePerSite = 5m,
                TotalOverseasSitesCharges = 10m,
                TonnageBandCharge = 100m,
                TotalAccreditationFees = 110m
            };

            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(It.IsAny<AccreditationFeesRequestDto>(),
                                                              It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            var actual = await _service.CalculateAccreditationFeesAsync(SampleRequest(), CancellationToken.None);

            // Assert
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ThrowsValidationException_WhenHttpServiceThrowsValidationException()
        {
            // Arrange
            var msg = "Invalid input";
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(It.IsAny<AccreditationFeesRequestDto>(),
                                                              It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.ComponentModel.DataAnnotations.ValidationException(msg));

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<System.ComponentModel.DataAnnotations.ValidationException>(
                () => _service.CalculateAccreditationFeesAsync(SampleRequest(), CancellationToken.None));
            Assert.AreEqual(msg, ex.Message);
        }

        [TestMethod]
        public async Task CalculateAccreditationFeesAsync_ThrowsServiceException_WhenHttpServiceThrowsUnexpectedException()
        {
            // Arrange
            var inner = new InvalidOperationException("Remote failure");
            _httpMock
                .Setup(x => x.CalculateAccreditationFeesAsync(It.IsAny<AccreditationFeesRequestDto>(),
                                                              It.IsAny<CancellationToken>()))
                .ThrowsAsync(inner);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ServiceException>(
                () => _service.CalculateAccreditationFeesAsync(SampleRequest(), CancellationToken.None));
            Assert.AreEqual(ExceptionMessages.ErrorCalculatingAccreditationFees, ex.Message);
            Assert.AreSame(inner, ex.InnerException);
        }
    }
}
