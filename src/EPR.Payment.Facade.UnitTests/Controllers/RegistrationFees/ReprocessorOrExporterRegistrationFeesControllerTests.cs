using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.RegistrationFees.ReProcessorOrExporter;
using FluentAssertions.Execution;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using EPR.Payment.Facade.Services.RegistrationFees.ReprocessorOrExporter.Interfaces;

namespace EPR.Payment.Facade.UnitTests.Controllers.RegistrationFees
{
    [TestClass]
    public class ReprocessorOrExporterRegistrationFeesControllerTests
    {
        [TestMethod, AutoMoqData]
        public void Constructor_WithValidArguments_ShouldInitializeCorrectly(
            [Frozen] Mock<IReprocessorExporterRegistrationFeesService> reprocessorExpoRegFeesServiceeMock,
            [Frozen] Mock<ILogger<ReprocessorOrExporterRegistrationFeesController>> loggerMock,
            [Frozen] Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>> validator)
        {
            // Act
            var controller = new ReprocessorOrExporterRegistrationFeesController(
                reprocessorExpoRegFeesServiceeMock.Object,
                loggerMock.Object,
                validator.Object
            );

            // Assert
            controller.Should().NotBeNull();
            controller.Should().BeAssignableTo<ReprocessorOrExporterRegistrationFeesController>();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullReproExpoRegFeeService_ShouldThrowArgumentNullException(
            [Frozen] Mock<ILogger<ReprocessorOrExporterRegistrationFeesController>> loggerMock,
            [Frozen] Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>> validator)
        {
            // Act
            Action act = () => new ReprocessorOrExporterRegistrationFeesController(
                null!,
                loggerMock.Object,
                validator.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("ReprocessorOrExporter Registration FeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException(
            [Frozen] Mock<IReprocessorExporterRegistrationFeesService> reprocessorExpoRegFeesServiceeMock,
            [Frozen] Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>> validator)
        {
            // Act
            Action act = () => new ReprocessorOrExporterRegistrationFeesController(
                reprocessorExpoRegFeesServiceeMock.Object,
                null!,
                validator.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullReproExpoRegistrationValidator_ShouldThrowArgumentNullException(
            [Frozen] Mock<IReprocessorExporterRegistrationFeesService> reprocessorExpoRegFeesServiceeMock,
            [Frozen] Mock<ILogger<ReprocessorOrExporterRegistrationFeesController>> loggerMock)
        {
            // Act
            Action act = () => new ReprocessorOrExporterRegistrationFeesController(
                reprocessorExpoRegFeesServiceeMock.Object,
                loggerMock.Object,
                null!
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("validator");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ValidRequest_ReturnsOk(
            [Frozen] Mock<IReprocessorExporterRegistrationFeesService> reprocessorExpoRegFeesServiceeMock,
            [Frozen] Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>> validatorMock,
            [Greedy] ReprocessorOrExporterRegistrationFeesController controller,
            [Frozen] ReprocessorOrExporterRegistrationFeesRequestDto request,
            [Frozen] ReprocessorOrExporterRegistrationFeesResponseDto expectedResponse)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            reprocessorExpoRegFeesServiceeMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.CalculateFeesAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                var okResult = result as OkObjectResult;
                okResult?.Value.Should().BeEquivalentTo(expectedResponse);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_InvalidRequest_ReturnsBadRequest(
            [Frozen] Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>> validatorMock,
            [Greedy] ReprocessorOrExporterRegistrationFeesController controller,
            [Frozen] ReprocessorOrExporterRegistrationFeesRequestDto request)
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("MemberType", ValidationMessages.InvalidMemberType)
            };
            var validationResult = new ValidationResult(validationFailures);
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            // Act
            var result = await controller.CalculateFeesAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                var problemDetails = badRequestResult?.Value as ProblemDetails;

                problemDetails.Should().NotBeNull();
                problemDetails?.Title.Should().Be("Validation Error");
                problemDetails?.Detail.Should().Contain(ValidationMessages.InvalidMemberType);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ServiceThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IReprocessorExporterRegistrationFeesService> reprocessorExpoRegFeesServiceeMock,
            [Frozen] Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>> validatorMock,
            [Greedy] ReprocessorOrExporterRegistrationFeesController controller,
            [Frozen] ReprocessorOrExporterRegistrationFeesRequestDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var validationException = new ValidationException("Validation error");
            reprocessorExpoRegFeesServiceeMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act
            var result = await controller.CalculateFeesAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                var problemDetails = badRequestResult?.Value as ProblemDetails;

                problemDetails.Should().NotBeNull();
                problemDetails?.Title.Should().Be("Validation Error");
                problemDetails?.Detail.Should().Be(validationException.Message);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ServiceThrowsServiceException_ReturnsInternalServerError(
            [Frozen] Mock<IReprocessorExporterRegistrationFeesService> reprocessorExpoRegFeesServiceeMock,
            [Frozen] Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>> validatorMock,
            [Greedy] ReprocessorOrExporterRegistrationFeesController controller,
            [Frozen] ReprocessorOrExporterRegistrationFeesRequestDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var serviceException = new ServiceException(ExceptionMessages.ErroreproExpoRegServiceFee);
            reprocessorExpoRegFeesServiceeMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(serviceException);

            // Act
            var result = await controller.CalculateFeesAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ObjectResult>();
                var objectResult = result as ObjectResult;
                var problemDetails = objectResult?.Value as ProblemDetails;

                objectResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
                problemDetails.Should().NotBeNull();
                problemDetails?.Title.Should().Be("Service Error");
                problemDetails?.Detail.Should().Be(ExceptionMessages.ErroreproExpoRegServiceFee);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ServiceThrowsException_ReturnsInternalServerError(
            [Frozen] Mock<IReprocessorExporterRegistrationFeesService> reprocessorExpoRegFeesServiceeMock,
            [Frozen] Mock<IValidator<ReprocessorOrExporterRegistrationFeesRequestDto>> validatorMock,
            [Greedy] ReprocessorOrExporterRegistrationFeesController controller,
            [Frozen] ReprocessorOrExporterRegistrationFeesRequestDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var exception = new Exception("Unexpected error");
            reprocessorExpoRegFeesServiceeMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            var result = await controller.CalculateFeesAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ObjectResult>();
                var objectResult = result as ObjectResult;
                var problemDetails = objectResult?.Value as ProblemDetails;

                objectResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
                problemDetails.Should().NotBeNull();
                problemDetails?.Title.Should().Be("Unexpected Error");
                problemDetails?.Detail.Should().Be(ExceptionMessages.UnexpectedErroreproExpoRegServiceFees);
            }
        }
    }
}