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

namespace EPR.Payment.Facade.UnitTests.Services.AccreditationFees;

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
    public async Task CalculateAccreditationFeesAsync_ReturnsDto_WhenHttpServiceSucceeds()
    {
        // Arrange
        var expected = new ReprocessorOrExporterAccreditationFeesResponseDto
        {
            OverseasSiteChargePerSite = 5m,
            TotalOverseasSitesCharges = 15m,
            TonnageBandCharge         = 50m,
            TotalAccreditationFees    = 65m
        };
        _httpMock
            .Setup(x => x.CalculateAccreditationFeesAsync(
                It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var actual = await _service.CalculateAccreditationFeesAsync(SampleRequest(), CancellationToken.None);

        // Assert
        Assert.AreSame(expected, actual);
    }

    [TestMethod]
    public async Task CalculateAccreditationFeesAsync_ReturnsNull_WhenHttpServiceReturnsNull()
    {
        // Arrange
        _httpMock
            .Setup(x => x.CalculateAccreditationFeesAsync(
                It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReprocessorOrExporterAccreditationFeesResponseDto?)null);

        // Act
        var actual = await _service.CalculateAccreditationFeesAsync(SampleRequest(), CancellationToken.None);

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task CalculateAccreditationFeesAsync_ThrowsValidationException_OnHttpValidationException()
    {
        // Arrange
        var message = "Invalid payload";
        _httpMock
            .Setup(x => x.CalculateAccreditationFeesAsync(
                It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(message)); // DataAnnotations.ValidationException

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<ValidationException>(
            () => _service.CalculateAccreditationFeesAsync(SampleRequest(), CancellationToken.None));

        Assert.AreEqual(message, ex.Message);
    }

    [TestMethod]
    public async Task CalculateAccreditationFeesAsync_ThrowsServiceException_OnGenericException()
    {
        // Arrange
        var inner = new InvalidOperationException("HTTP failure");
        _httpMock
            .Setup(x => x.CalculateAccreditationFeesAsync(
                It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(inner);

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<ServiceException>(
            () => _service.CalculateAccreditationFeesAsync(SampleRequest(), CancellationToken.None));

        Assert.AreEqual(ExceptionMessages.ErrorCalculatingAccreditationFees, ex.Message);
        Assert.AreSame(inner, ex.InnerException);
    }
}
