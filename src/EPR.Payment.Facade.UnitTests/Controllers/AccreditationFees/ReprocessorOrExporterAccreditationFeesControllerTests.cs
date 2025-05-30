using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Enums;
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
        private readonly Mock<ILogger<ReprocessorOrExporterAccreditationFeesController>> _mockLogger = new();
        private readonly Mock<IAccreditationFeesCalculatorService> _mockAccreditationFeesCalculatorService = new();

        private ReprocessorOrExporterAccreditationFeesController? _reprocessorOrExporterAccreditationFeesControllerUnderTest;

        private CancellationToken _cancellationToken;

        [TestInitialize]
        public void Setup()
        {
            _reprocessorOrExporterAccreditationFeesControllerUnderTest = new ReprocessorOrExporterAccreditationFeesController(
                _mockLogger.Object,
                _mockValidator.Object,
                _mockAccreditationFeesCalculatorService.Object
            );

            _reprocessorOrExporterAccreditationFeesControllerUnderTest.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _cancellationToken = new CancellationToken();
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            var request = new ReprocessorOrExporterAccreditationFeesRequestDto
            {
                Regulator = "GN-ENG",
                TonnageBand = TonnageBands.Upto500,
                NumberOfOverseasSites = 10,
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = DateTime.UtcNow
            };

            var failures = new List<ValidationFailure>
            {
                new ("RequestorType", "RequestorType is required")
            };

            // Setup
            _mockValidator
                .Setup(v => v.Validate(request))
                .Returns(new ValidationResult(failures));

            // Act
            IActionResult result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, _cancellationToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var problems = badRequest.Value as ProblemDetails;
            Assert.IsNotNull(problems);
            Assert.IsTrue(problems.Detail!.Contains("RequestorType is required"));
            Assert.AreEqual(StatusCodes.Status400BadRequest, problems.Status);

            // Verify
            _mockValidator
               .Verify(v => v.Validate(request), Times.Once());
            _mockAccreditationFeesCalculatorService.Verify(x => x.CalculateAccreditationFeesAsync(
                request,
                _cancellationToken), Times.Never());
        }

        [TestMethod]
        public async Task GetAccreditationFee_ShouldReturnsOk_WhenRequestIsValid_AndServiceCallReturnResponse()
        {
            // Arrange
            var request = new ReprocessorOrExporterAccreditationFeesRequestDto
            {
                RequestorType = RequestorTypes.Exporters,
                Regulator = "GN-ENG",
                TonnageBand = TonnageBands.Upto500,
                NumberOfOverseasSites = 10,
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = DateTime.UtcNow
            };

            var response = new ReprocessorOrExporterAccreditationFeesResponseDto()
            {
                OverseasSiteChargePerSite = 10,
                TonnageBandCharge = 100,
                TotalOverseasSitesCharges = 100,
                TotalAccreditationFees = 200,
            };

            // Setup
            _mockValidator
                .Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>()))
                .Returns(new ValidationResult());
            _mockAccreditationFeesCalculatorService.Setup(x => x.CalculateAccreditationFeesAsync(
                request,
                _cancellationToken))
                .ReturnsAsync(response);

            // Act
            IActionResult result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, _cancellationToken);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var accreditationFeesResponseDto = okResult.Value as ReprocessorOrExporterAccreditationFeesResponseDto;
            Assert.IsNotNull(accreditationFeesResponseDto);
            Assert.AreSame(accreditationFeesResponseDto, response);

            // Verify
            _mockValidator
               .Verify(v => v.Validate(request), Times.Once());
            _mockAccreditationFeesCalculatorService.Verify(x => x.CalculateAccreditationFeesAsync(
                request,
                _cancellationToken), Times.Once());
        }
    }
}
