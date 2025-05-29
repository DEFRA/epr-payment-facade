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
        private readonly Mock<IValidator<AccreditationFeesRequestDto>> _mockValidator = new();
        private readonly Mock<ILogger<ReprocessorOrExporterAccreditationFeesController>> _mockLogger = new();
        private readonly Mock<IAccreditationFeesCalculatorService> _mockAccreditationFeesCalculatorService = new();

        private ReprocessorOrExporterAccreditationFeesController? _reprocessorOrExporterAccreditationFeesControllerUnderTest;

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
        }

        [TestMethod]
        public async Task GetAccreditationFee_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("RequestorType", "RequestorType is required")
            };
            _mockValidator
                .Setup(v => v.Validate(It.IsAny<AccreditationFeesRequestDto>()))
                .Returns(new ValidationResult(failures));

            var request = new AccreditationFeesRequestDto
            {
                Regulator = "GN-ENG",
                TonnageBand = TonnageBands.Upto500,
                NumberOfOverseasSites = 10,
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = DateTime.UtcNow
            };

            // Act
            var result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var problems = badRequest.Value as ProblemDetails;
            Assert.IsNotNull(problems);
            Assert.IsTrue(problems.Detail!.Contains("RequestorType is required"));
            Assert.AreEqual(StatusCodes.Status400BadRequest, problems.Status);
        }

        [TestMethod]
        public async Task GetAccreditationFee_ReturnsOk_WhenRequestIsValid()
        {
            // Arrange
            _mockValidator
                .Setup(v => v.Validate(It.IsAny<AccreditationFeesRequestDto>()))
                .Returns(new ValidationResult());

            var request = new AccreditationFeesRequestDto
            {
                RequestorType = RequestorTypes.Exporters,
                Regulator = "GN-ENG",
                TonnageBand = TonnageBands.Upto500,
                NumberOfOverseasSites = 10,
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = DateTime.UtcNow
            };

            // Act
            var result = await _reprocessorOrExporterAccreditationFeesControllerUnderTest!.GetAccreditationFee(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var response = okResult.Value as AccreditationFeesResponseDto;
            Assert.IsNotNull(response);

            // Validate the dummy values from controller
            Assert.AreEqual(75.00m, response.OverseasSiteChargePerSite);
            Assert.AreEqual(225.00m, response.TotalOverseasSitesCharges);
            Assert.AreEqual(310.00m, response.TonnageBandCharge);

            Assert.IsNotNull(response.PreviousPaymentDetail);
           
        }
    }
}
