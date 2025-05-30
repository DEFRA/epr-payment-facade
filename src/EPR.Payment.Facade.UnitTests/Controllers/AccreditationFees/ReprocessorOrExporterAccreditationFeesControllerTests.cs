using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Controllers.AccreditationFees;
using EPR.Payment.Facade.Services.AccreditationFees.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers.AccreditationFees;

[TestClass]
public class ReprocessorOrExporterAccreditationFeesControllerTests
{
    Mock<IValidator<ReprocessorOrExporterAccreditationFeesRequestDto>> _validator = null!;
    Mock<IAccreditationFeesCalculatorService> _service = null!;
    Mock<ILogger<ReprocessorOrExporterAccreditationFeesController>> _logger = null!;
    ReprocessorOrExporterAccreditationFeesController _controller = null!;

    static ReprocessorOrExporterAccreditationFeesRequestDto NewRequest() =>
        new()
        {
            RequestorType              = RequestorTypes.Exporters,
            Regulator                  = "TEST-REG",
            TonnageBand                = TonnageBands.Upto500,
            NumberOfOverseasSites      = 2,
            MaterialType               = MaterialTypes.Plastic,
            ApplicationReferenceNumber = "REF123",
            SubmissionDate             = DateTime.UtcNow
        };

    [TestInitialize]
    public void Init()
    {
        _validator  = new Mock<IValidator<ReprocessorOrExporterAccreditationFeesRequestDto>>();
        _service    = new Mock<IAccreditationFeesCalculatorService>();
        _logger     = new Mock<ILogger<ReprocessorOrExporterAccreditationFeesController>>();

        _controller = new ReprocessorOrExporterAccreditationFeesController(
            _logger.Object,
            _validator.Object,
            _service.Object
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task GetAccreditationFee_BadRequest_OnValidationFailure()
    {
        // arrange
        var failures = new[] { new ValidationFailure("Foo", "Foo required") };
        _validator.Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>()))
                  .Returns(new ValidationResult(failures));

        // act
        var result = await _controller.GetAccreditationFee(NewRequest(), CancellationToken.None);

        // assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var pd = ((BadRequestObjectResult)result).Value as ProblemDetails;
        Assert.AreEqual(400, pd!.Status);
        StringAssert.Contains(pd.Detail!, "Foo required");
    }

    [TestMethod]
    public async Task GetAccreditationFee_NotFound_WhenServiceReturnsNull()
    {
        // arrange
        _validator.Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>()))
                  .Returns(new ValidationResult());
        _service.Setup(s => s.CalculateAccreditationFeesAsync(
                            It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReprocessorOrExporterAccreditationFeesResponseDto?)null);

        // act
        var result = await _controller.GetAccreditationFee(NewRequest(), CancellationToken.None);

        // assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        var pd = ((NotFoundObjectResult)result).Value as ProblemDetails;
        Assert.AreEqual(404, pd!.Status);
        StringAssert.Contains(pd.Detail!, "not found");
    }

    [TestMethod]
    public async Task GetAccreditationFee_Ok_WhenServiceReturnsDto()
    {
        // arrange
        _validator.Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>()))
                  .Returns(new ValidationResult());

        var expected = new ReprocessorOrExporterAccreditationFeesResponseDto
        {
            OverseasSiteChargePerSite = 10m,
            TotalOverseasSitesCharges = 20m,
            TonnageBandCharge         = 100m,
            TotalAccreditationFees    = 130m,
            PreviousPaymentDetail     = new PreviousPaymentDetailResponseDto
            {
                PaymentMode   = "Online",
                PaymentMethod = "Credit",
                PaymentDate   = DateTime.UtcNow.AddDays(-1),
                PaymentAmount = 30m
            }
        };
        _service.Setup(s => s.CalculateAccreditationFeesAsync(
                            It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

        // act
        var result = await _controller.GetAccreditationFee(NewRequest(), CancellationToken.None);

        // assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var actual = ((OkObjectResult)result).Value as ReprocessorOrExporterAccreditationFeesResponseDto;
        Assert.AreEqual(expected.TotalAccreditationFees, actual!.TotalAccreditationFees);
        Assert.AreEqual(expected.PreviousPaymentDetail.PaymentAmount, actual.PreviousPaymentDetail!.PaymentAmount);
    }

    [TestMethod]
    public async Task GetAccreditationFee_BadRequest_OnServiceValidationException()
    {
        // arrange
        _validator.Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>()))
                  .Returns(new ValidationResult());
        _service.Setup(s => s.CalculateAccreditationFeesAsync(
                            It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>(),
                            It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Bad payload"));

        // act
        var result = await _controller.GetAccreditationFee(NewRequest(), CancellationToken.None);

        // assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var pd = ((BadRequestObjectResult)result).Value as ProblemDetails;
        Assert.AreEqual(400, pd!.Status);
        StringAssert.Contains(pd.Detail!, "Bad payload");
    }

    [TestMethod]
    public async Task GetAccreditationFee_ServiceError_OnServiceException()
    {
        // arrange
        _validator.Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>()))
                  .Returns(new ValidationResult());
        _service.Setup(s => s.CalculateAccreditationFeesAsync(
                            It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>(),
                            It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ServiceException("Downstream"));

        // act
        var result = await _controller.GetAccreditationFee(NewRequest(), CancellationToken.None);

        // assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var or = (ObjectResult)result;
        Assert.AreEqual(500, or.StatusCode);
        var pd = or.Value as ProblemDetails;
        Assert.AreEqual("Downstream", pd!.Detail);
    }

    [TestMethod]
    public async Task GetAccreditationFee_UnexpectedError_OnUnhandledException()
    {
        // arrange
        _validator.Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>()))
                  .Returns(new ValidationResult());
        _service.Setup(s => s.CalculateAccreditationFeesAsync(
                            It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>(),
                            It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Boom"));

        // act
        var result = await _controller.GetAccreditationFee(NewRequest(), CancellationToken.None);

        // assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var or = (ObjectResult)result;
        Assert.AreEqual(500, or.StatusCode);
        var pd = or.Value as ProblemDetails;
        Assert.AreEqual(ExceptionMessages.UnexpectedErrorCalculatingFees, pd!.Detail);
    }
}
