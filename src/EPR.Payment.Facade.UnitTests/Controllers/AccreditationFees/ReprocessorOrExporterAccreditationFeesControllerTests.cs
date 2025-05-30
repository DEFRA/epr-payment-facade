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

namespace EPR.Payment.Facade.UnitTests.Controllers.AccreditationFees
{
    [TestClass]
    public class ReprocessorOrExporterAccreditationFeesControllerTests
    {
        private readonly Mock<IValidator<ReprocessorOrExporterAccreditationFeesRequestDto>> _mockValidator = new();
        private readonly Mock<IAccreditationFeesCalculatorService> _mockService= new();
        private readonly Mock<ILogger<ReprocessorOrExporterAccreditationFeesController>> _mockLogger= new();
        private ReprocessorOrExporterAccreditationFeesController? _reprocessorOrExporterAccreditationFeesControllerUnderTest;
        private CancellationToken _cancellationToken;

        private static ReprocessorOrExporterAccreditationFeesRequestDto AccreditationFeesRequestDto() =>
            new()
            {
                RequestorType              = RequestorTypes.Exporters,
                Regulator                  = "GN-ENG",
                TonnageBand                = TonnageBands.Upto500,
                NumberOfOverseasSites      = 10,
                MaterialType               = MaterialTypes.Plastic,
                ApplicationReferenceNumber = "REF123",
                SubmissionDate             = DateTime.UtcNow
            };

        [TestInitialize]
        public void Setup()
        {
            _reprocessorOrExporterAccreditationFeesControllerUnderTest = new(
                _mockLogger.Object,
                _mockValidator.Object,
                _mockService.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            _cancellationToken = new CancellationToken();
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            ReprocessorOrExporterAccreditationFeesRequestDto request = AccreditationFeesRequestDto();
            var failures = new List<ValidationFailure>
            {
                new("RequestorType", "RequestorType is required")
            };

            // Setup
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult(failures));

            // Act
            IActionResult result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, _cancellationToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            BadRequestObjectResult bad = (BadRequestObjectResult)result;
            ProblemDetails pd = (ProblemDetails)bad.Value!;
            Assert.AreEqual(StatusCodes.Status400BadRequest, pd.Status);
            StringAssert.Contains(pd.Detail!, "RequestorType is required");

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once());
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Never());
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnNotFound_WhenServiceReturnsNull()
        {
            // Arrange
            ReprocessorOrExporterAccreditationFeesRequestDto request = AccreditationFeesRequestDto();

            // Setup
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken))
                .ReturnsAsync((ReprocessorOrExporterAccreditationFeesResponseDto?)null);

            // Act
            IActionResult result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, _cancellationToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            NotFoundObjectResult nf = (NotFoundObjectResult)result;
            ProblemDetails pd = (ProblemDetails)nf.Value!;
            Assert.AreEqual(StatusCodes.Status404NotFound, pd.Status);
            StringAssert.Contains(pd.Detail!, "Accreditation fees data not found.");

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once());
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Once());
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnOk_WhenServiceReturnsDto()
        {
            // Arrange
            ReprocessorOrExporterAccreditationFeesRequestDto request = AccreditationFeesRequestDto();
            ReprocessorOrExporterAccreditationFeesResponseDto expected = new()
            {
                OverseasSiteChargePerSite   = 10m,
                TotalOverseasSitesCharges   = 100m,
                TonnageBandCharge           = 200m,
                TotalAccreditationFees      = 310m,
                PreviousPaymentDetail       = new PreviousPaymentDetailResponseDto
                {
                    PaymentMode   = "Online",
                    PaymentMethod = "Card",
                    PaymentDate   = DateTime.UtcNow.AddDays(-1),
                    PaymentAmount = 150m
                }
            };
            
            // Setup
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken))
                .ReturnsAsync(expected);

            // Act
            IActionResult result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, _cancellationToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult ok = (OkObjectResult)result;
            ReprocessorOrExporterAccreditationFeesResponseDto actual =
                (ReprocessorOrExporterAccreditationFeesResponseDto)ok.Value!;
            Assert.AreSame(expected, actual);

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once());
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Once());
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnBadRequest_OnServiceValidationException()
        {
            // Arrange
            ReprocessorOrExporterAccreditationFeesRequestDto request = AccreditationFeesRequestDto();

            // Setup
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken))
                .ThrowsAsync(new FluentValidation.ValidationException("Bad payload"));

            // Act
            IActionResult result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, _cancellationToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            BadRequestObjectResult bad = (BadRequestObjectResult)result;
            ProblemDetails pd = (ProblemDetails)bad.Value!;
            Assert.AreEqual(StatusCodes.Status400BadRequest, pd.Status);
            StringAssert.Contains(pd.Detail!, "Bad payload");

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once());
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Once());
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnServiceError_OnServiceException()
        {
            // Arrange
            ReprocessorOrExporterAccreditationFeesRequestDto request = AccreditationFeesRequestDto();

            // Setup
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken))
                .ThrowsAsync(new ServiceException("Downstream error"));

            // Act
            IActionResult result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, _cancellationToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            ObjectResult or = (ObjectResult)result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, or.StatusCode);
            ProblemDetails pd = (ProblemDetails)or.Value!;
            Assert.AreEqual("Downstream error", pd.Detail);

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once());
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Once());
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnUnexpectedError_OnUnhandledException()
        {
            // Arrange
            ReprocessorOrExporterAccreditationFeesRequestDto request = AccreditationFeesRequestDto();

            // Setup
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken))
                .ThrowsAsync(new Exception("Boom"));

            // Act
            IActionResult result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, _cancellationToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            ObjectResult or = (ObjectResult)result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, or.StatusCode);
            ProblemDetails pd = (ProblemDetails)or.Value!;
            Assert.AreEqual(ExceptionMessages.UnexpectedErrorCalculatingFees, pd.Detail);

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once());
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _cancellationToken), Times.Once());
        }
    }
}
