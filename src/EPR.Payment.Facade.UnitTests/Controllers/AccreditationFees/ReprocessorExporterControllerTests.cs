using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Controllers.AccreditationFees;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers.AccreditationFees
{
    [TestClass]
    public class ReprocessorExporterControllerTests
    {
        private Mock<IValidator<AccreditationFeesRequestDto>> _mockValidator;
        private Mock<ILogger<ReprocessorExporterController>> _mockLogger;
        private ReprocessorExporterController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockValidator = new Mock<IValidator<AccreditationFeesRequestDto>>();
            _mockLogger = new Mock<ILogger<ReprocessorExporterController>>();

            _controller = new ReprocessorExporterController(
                _mockLogger.Object,
                _mockValidator.Object
            );

            _controller.ControllerContext = new ControllerContext
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
                new ValidationFailure("SubmissionDate", "SubmissionDate is required")
            };
            _mockValidator
                .Setup(v => v.Validate(It.IsAny<AccreditationFeesRequestDto>()))
                .Returns(new ValidationResult(failures));

            var request = new AccreditationFeesRequestDto
            {
                SubmissionDate = DateTime.UtcNow
            };

            // Act
            var result = await _controller.GetAccreditationFee(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var problems = badRequest.Value as ProblemDetails;
            Assert.IsNotNull(problems);
            Assert.IsTrue(problems.Detail.Contains("SubmissionDate is required"));
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
                SubmissionDate = DateTime.UtcNow
            };

            // Act
            var result = await _controller.GetAccreditationFee(request, CancellationToken.None);

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
            Assert.AreEqual("offline", response.PreviousPaymentDetail.PaymentMode);
            Assert.AreEqual("bank transfer", response.PreviousPaymentDetail.PaymentMethod);
            Assert.AreEqual(200.00m, response.PreviousPaymentDetail.PaymentAmount);
            Assert.AreEqual(new DateTime(2024, 11, 15), response.PreviousPaymentDetail.PaymentDate);
            Assert.AreEqual(new DateTime(2024, 11, 15), response.PreviousPaymentDetail.PaymentDate);
        }
    }
}
