using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.AccreditationFees;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
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
        private Mock<IValidator<AccreditationFeesRequestDto>> _validatorMock;
        private Mock<IAccreditationFeesCalculatorService> _serviceMock;
        private Mock<ILogger<ReprocessorOrExporterAccreditationFeesController>> _loggerMock;
        private ReprocessorOrExporterAccreditationFeesController _controller;

        private static AccreditationFeesRequestDto CreateValidRequest() => new AccreditationFeesRequestDto
        {
            RequestorType = RequestorTypes.Exporters,
            Regulator = "TEST-REG",
            TonnageBand = TonnageBands.Upto500,
            NumberOfOverseasSites = 1,
            MaterialType = MaterialTypes.Plastic,
            ApplicationReferenceNumber = "REF123",
            SubmissionDate = DateTime.UtcNow
        };

        private CancellationToken _cancellationToken;

        [TestInitialize]
        public void Setup()
        {
            _validatorMock = new Mock<IValidator<AccreditationFeesRequestDto>>();
            _serviceMock   = new Mock<IAccreditationFeesCalculatorService>();
            _loggerMock    = new Mock<ILogger<ReprocessorOrExporterAccreditationFeesController>>();

            _controller = new ReprocessorOrExporterAccreditationFeesController(
                _loggerMock.Object,
                _validatorMock.Object,
                _serviceMock.Object
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
        public async Task GetAccreditationFee_ReturnsBadRequest_WhenValidatorReturnsErrors()
        {
            // Arrange
            var request = new ReprocessorOrExporterAccreditationFeesRequestDto
            {
                new ValidationFailure("Foo", "Foo is required"),
                new ValidationFailure("Bar", "Bar must be > 0")
            };
            _validatorMock
                .Setup(v => v.Validate(It.IsAny<AccreditationFeesRequestDto>()))
                .Returns(new ValidationResult(failures));

            var request = CreateValidRequest();

            // Act
            var result = await _controller.GetAccreditationFee(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = (BadRequestObjectResult)result;
            var details = (ProblemDetails)badRequest.Value!;
            Assert.AreEqual(StatusCodes.Status400BadRequest, details.Status);
            StringAssert.Contains(details.Detail!, "Foo is required");
            StringAssert.Contains(details.Detail!, "Bar must be > 0");
        }

        [TestMethod]
        public async Task GetAccreditationFee_ReturnsBadRequest_WhenServiceThrowsValidationException()
        {
            // Arrange
            _validatorMock
                .Setup(v => v.Validate(It.IsAny<AccreditationFeesRequestDto>()))
                .Returns(new ValidationResult()); // valid

            _serviceMock
                .Setup(s => s.CalculateAccreditationFeesAsync(
                    It.IsAny<AccreditationFeesRequestDto>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Service-level validation failed"));

            var request = CreateValidRequest();

            // Act
            var result = await _controller.GetAccreditationFee(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequest = (BadRequestObjectResult)result;
            var details = (ProblemDetails)badRequest.Value!;
            Assert.AreEqual(StatusCodes.Status400BadRequest, details.Status);
            StringAssert.Contains(details.Detail!, "Service-level validation failed");
        }

        [TestMethod]
        public async Task GetAccreditationFee_ReturnsServerError_WhenServiceThrowsUnexpectedException()
        {
            // Arrange
            _validatorMock
                .Setup(v => v.Validate(It.IsAny<AccreditationFeesRequestDto>()))
                .Returns(new ValidationResult()); // valid

            _serviceMock
                .Setup(s => s.CalculateAccreditationFeesAsync(
                    It.IsAny<AccreditationFeesRequestDto>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Boom!"));

            var request = CreateValidRequest();

            // Setup
            _mockValidator
                .Setup(v => v.Validate(It.IsAny<ReprocessorOrExporterAccreditationFeesRequestDto>()))
                .Returns(new ValidationResult());
            _mockAccreditationFeesCalculatorService.Setup(x => x.CalculateAccreditationFeesAsync(
                request,
                _cancellationToken))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAccreditationFee(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var details = (ProblemDetails)objectResult.Value!;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, details.Status);
            Assert.AreEqual(ExceptionMessages.UnexpectedErrorCalculatingFees, details.Detail);
        }

        [TestMethod]
        public async Task GetAccreditationFee_ReturnsOk_WhenServiceReturnsResponse()
        {
            // Arrange
            _validatorMock
                .Setup(v => v.Validate(It.IsAny<AccreditationFeesRequestDto>()))
                .Returns(new ValidationResult()); // valid

            var expectedDto = new AccreditationFeesResponseDto
            {
                OverseasSiteChargePerSite   = 10m,
                TotalOverseasSitesCharges   = 30m,
                TonnageBandCharge           = 100m,
                TotalAccreditationFees      = 140m,
                PreviousPaymentDetail       = new PreviousPaymentDetailResponseDto
                {
                    PaymentMode   = "Online",
                    PaymentMethod = "Credit Card",
                    PaymentDate   = DateTime.UtcNow.AddDays(-7),
                    PaymentAmount = 25m
                }
            };

            _serviceMock
                .Setup(s => s.CalculateAccreditationFeesAsync(
                    It.IsAny<AccreditationFeesRequestDto>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var request = CreateValidRequest();

            // Act
            var result = await _controller.GetAccreditationFee(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            var actual = (AccreditationFeesResponseDto)ok.Value!;
            Assert.AreEqual(expectedDto.OverseasSiteChargePerSite, actual.OverseasSiteChargePerSite);
            Assert.AreEqual(expectedDto.TotalOverseasSitesCharges, actual.TotalOverseasSitesCharges);
            Assert.AreEqual(expectedDto.TonnageBandCharge, actual.TonnageBandCharge);
            Assert.AreEqual(expectedDto.TotalAccreditationFees, actual.TotalAccreditationFees);
            Assert.IsNotNull(actual.PreviousPaymentDetail);
            Assert.AreEqual(expectedDto.PreviousPaymentDetail.PaymentAmount, actual.PreviousPaymentDetail!.PaymentAmount);
            Assert.AreEqual(expectedDto.PreviousPaymentDetail.PaymentMode, actual.PreviousPaymentDetail.PaymentMode);
            Assert.AreEqual(expectedDto.PreviousPaymentDetail.PaymentMethod, actual.PreviousPaymentDetail.PaymentMethod);
            Assert.AreEqual(expectedDto.PreviousPaymentDetail.PaymentDate, actual.PreviousPaymentDetail.PaymentDate);
        }
    }
}
