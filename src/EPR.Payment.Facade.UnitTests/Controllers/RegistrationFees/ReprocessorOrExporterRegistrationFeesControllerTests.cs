using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Controllers.RegistrationFees.ReProcessorOrExporter;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace EPR.Payment.Facade.UnitTests.Controllers.RegistrationFees
{

    [TestClass]
    public class ReprocessorOrExporterRegistrationFeesControllerTests
    {
        private Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>> _mockValidator;
        private Mock<ILogger<ReprocessorOrExporterRegistrationFeesController>> _mockLogger;
        private ReprocessorOrExporterRegistrationFeesController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockValidator = new Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>>();
            _mockLogger = new Mock<ILogger<ReprocessorOrExporterRegistrationFeesController>>();

            _controller = new ReprocessorOrExporterRegistrationFeesController(
            _mockLogger.Object,
            _mockValidator.Object
            );

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                    new Claim(ClaimTypes.Name, "testuser")
                }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            _mockValidator.Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterRegistrationFeesRequestDto>()))
            .Returns(new ValidationResult(new List<ValidationFailure>
            {
                    new ValidationFailure("MaterialType", "MaterialType is required")
            }));

            var request = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = "Reprocessor",
                Regulator = "GB-ENG",
                SubmissionDate = DateTime.UtcNow,
                MaterialType = ""
            };

            // Act
            var result = await _controller.CalculateFeesAsync(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var problem = (ProblemDetails)((BadRequestObjectResult)result).Value;
            Assert.IsTrue(problem.Detail.Contains("MaterialType is required"));
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ReturnsOk_WhenRequestIsValid()
        {
            // Arrange
            _mockValidator.Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterRegistrationFeesRequestDto>()))
 .Returns(new ValidationResult());

            var request = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = "Reprocessor",
                Regulator = "GB-ENG",
                SubmissionDate = DateTime.UtcNow,
                MaterialType = "Plastic"
            };

            // Act
            var result = await _controller.CalculateFeesAsync(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var response = (ReprocessorOrExporterRegistrationFeesResponseDto)((OkObjectResult)result).Value;
            Assert.AreEqual("Plastic", response.MaterialType);
            Assert.AreEqual(100.0m, response.RegistrationFee);
            Assert.IsNull(response.PreviousPaymentDetail);
        }
    }
}