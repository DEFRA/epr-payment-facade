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
        private readonly Mock<IValidator<ReprocessorOrExporterAccreditationFeesRequestDto>> _mockValidator
            = new Mock<IValidator<ReprocessorOrExporterAccreditationFeesRequestDto>>();
        private readonly Mock<IAccreditationFeesCalculatorService> _mockService
            = new Mock<IAccreditationFeesCalculatorService>();
        private readonly Mock<ILogger<ReprocessorOrExporterAccreditationFeesController>> _mockLogger
            = new Mock<ILogger<ReprocessorOrExporterAccreditationFeesController>>();

        private ReprocessorOrExporterAccreditationFeesController _controller = null!;
        private CancellationToken _ct;

        private static ReprocessorOrExporterAccreditationFeesRequestDto NewRequest() =>
            new ReprocessorOrExporterAccreditationFeesRequestDto
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
            _controller = new ReprocessorOrExporterAccreditationFeesController(
                _mockLogger.Object,
                _mockValidator.Object,
                _mockService.Object
            );
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _ct = new CancellationToken();
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var request = NewRequest();
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("RequestorType", "RequestorType is required")
            };
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult(failures));

            // Act
            IActionResult result = await _controller.GetAccreditationFee(request, _ct);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            BadRequestObjectResult bad = (BadRequestObjectResult)result;
            ProblemDetails pd = (ProblemDetails)bad.Value!;
            Assert.AreEqual(StatusCodes.Status400BadRequest, pd.Status);
            StringAssert.Contains(pd.Detail!, "RequestorType is required");

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once);
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _ct), Times.Never);
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnNotFound_WhenServiceReturnsNull()
        {
            // Arrange
            var request = NewRequest();
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _ct))
                .ReturnsAsync((ReprocessorOrExporterAccreditationFeesResponseDto?)null);

            // Act
            IActionResult result = await _controller.GetAccreditationFee(request, _ct);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            NotFoundObjectResult nf = (NotFoundObjectResult)result;
            ProblemDetails pd = (ProblemDetails)nf.Value!;
            Assert.AreEqual(StatusCodes.Status404NotFound, pd.Status);
            StringAssert.Contains(pd.Detail!, "Accreditation fees data not found.");

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once);
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _ct), Times.Once);
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnOk_WhenServiceReturnsDto()
        {
            // Arrange
            var request = NewRequest();
            var expected = new ReprocessorOrExporterAccreditationFeesResponseDto
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
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _ct))
                .ReturnsAsync(expected);

            // Act
            IActionResult result = await _controller.GetAccreditationFee(request, _ct);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult ok = (OkObjectResult)result;
            ReprocessorOrExporterAccreditationFeesResponseDto actual =
                (ReprocessorOrExporterAccreditationFeesResponseDto)ok.Value!;
            Assert.AreSame(expected, actual);

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once);
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _ct), Times.Once);
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnBadRequest_OnServiceValidationException()
        {
            // Arrange
            var request = NewRequest();
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _ct))
                .ThrowsAsync(new FluentValidation.ValidationException("Bad payload"));

            // Act
            IActionResult result = await _controller.GetAccreditationFee(request, _ct);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            BadRequestObjectResult bad = (BadRequestObjectResult)result;
            ProblemDetails pd = (ProblemDetails)bad.Value!;
            Assert.AreEqual(StatusCodes.Status400BadRequest, pd.Status);
            StringAssert.Contains(pd.Detail!, "Bad payload");

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once);
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _ct), Times.Once);
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnServiceError_OnServiceException()
        {
            // Arrange
            var request = NewRequest();
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _ct))
                .ThrowsAsync(new ServiceException("Downstream error"));

            // Act
            IActionResult result = await _controller.GetAccreditationFee(request, _ct);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            ObjectResult or = (ObjectResult)result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, or.StatusCode);
            ProblemDetails pd = (ProblemDetails)or.Value!;
            Assert.AreEqual("Downstream error", pd.Detail);

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once);
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _ct), Times.Once);
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnUnexpectedError_OnUnhandledException()
        {
            // Arrange
            var request = NewRequest();
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult());
            _mockService
                .Setup(s => s.CalculateAccreditationFeesAsync(request, _ct))
                .ThrowsAsync(new Exception("Boom"));

            // Act
            IActionResult result = await _controller.GetAccreditationFee(request, _ct);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            ObjectResult or = (ObjectResult)result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, or.StatusCode);
            ProblemDetails pd = (ProblemDetails)or.Value!;
            Assert.AreEqual(ExceptionMessages.UnexpectedErrorCalculatingFees, pd.Detail);

            // Verify
            _mockValidator.Verify(v => v.Validate(request), Times.Once);
            _mockService.Verify(s => s.CalculateAccreditationFeesAsync(request, _ct), Times.Once);
        }
    }
}
