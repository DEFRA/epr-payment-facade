using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.RegistrationFees;
using EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers.RegistrationFees
{
    [TestClass]
    public class ComplianceSchemeFeesControllerTests
    {
        [TestMethod, AutoMoqData]
        public void Constructor_WithValidArguments_ShouldInitializeCorrectly(
            [Frozen] Mock<IComplianceSchemeCalculatorService> complianceSchemeFeesServiceMock,
            [Frozen] Mock<ILogger<ComplianceSchemeFeesController>> loggerMock,
            [Frozen] Mock<IValidator<ComplianceSchemeFeesRequestDto>> validator)
        {
            // Act
            var controller = new ComplianceSchemeFeesController(
                complianceSchemeFeesServiceMock.Object,
                loggerMock.Object,
                validator.Object
            );

            // Assert
            controller.Should().NotBeNull();
            controller.Should().BeAssignableTo<ComplianceSchemeFeesController>();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullComplianceSchemeFeesService_ShouldThrowArgumentNullException(
            [Frozen] Mock<ILogger<ComplianceSchemeFeesController>> loggerMock,
            [Frozen] Mock<IValidator<ComplianceSchemeFeesRequestDto>> validator)
        {
            // Act
            Action act = () => new ComplianceSchemeFeesController(
                null!,
                loggerMock.Object,
                validator.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("complianceSchemeFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException(
            [Frozen] Mock<IComplianceSchemeCalculatorService> complianceSchemeFeesServiceMock,
            [Frozen] Mock<IValidator<ComplianceSchemeFeesRequestDto>> validator)
        {
            // Act
            Action act = () => new ComplianceSchemeFeesController(
                complianceSchemeFeesServiceMock.Object,
                null!,
                validator.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullComplianceSchemeValidator_ShouldThrowArgumentNullException(
            [Frozen] Mock<IComplianceSchemeCalculatorService> complianceSchemeFeesServiceMock,
            [Frozen] Mock<ILogger<ComplianceSchemeFeesController>> loggerMock)
        {
            // Act
            Action act = () => new ComplianceSchemeFeesController(
                complianceSchemeFeesServiceMock.Object,
                loggerMock.Object,
                null!
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("validator");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ValidRequest_ReturnsOk(
            [Frozen] Mock<IComplianceSchemeCalculatorService> complianceSchemeCalculatorServiceMock,
            [Frozen] Mock<IValidator<ComplianceSchemeFeesRequestDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] ComplianceSchemeFeesRequestDto request,
            [Frozen] ComplianceSchemeFeesResponseDto expectedResponse)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            complianceSchemeCalculatorServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
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
            [Frozen] Mock<IValidator<ComplianceSchemeFeesRequestDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] ComplianceSchemeFeesRequestDto request)
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
            [Frozen] Mock<IComplianceSchemeCalculatorService> complianceSchemeCalculatorServiceMock,
            [Frozen] Mock<IValidator<ComplianceSchemeFeesRequestDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] ComplianceSchemeFeesRequestDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var validationException = new ValidationException("Validation error");
            complianceSchemeCalculatorServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
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
            [Frozen] Mock<IComplianceSchemeCalculatorService> complianceSchemeCalculatorServiceMock,
            [Frozen] Mock<IValidator<ComplianceSchemeFeesRequestDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] ComplianceSchemeFeesRequestDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var serviceException = new ServiceException(ExceptionMessages.ErrorCalculatingComplianceSchemeFees);
            complianceSchemeCalculatorServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
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
                problemDetails?.Detail.Should().Be(ExceptionMessages.ErrorCalculatingComplianceSchemeFees);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ServiceThrowsException_ReturnsInternalServerError(
            [Frozen] Mock<IComplianceSchemeCalculatorService> complianceSchemeCalculatorServiceMock,
            [Frozen] Mock<IValidator<ComplianceSchemeFeesRequestDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] ComplianceSchemeFeesRequestDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var exception = new Exception("Unexpected error");
            complianceSchemeCalculatorServiceMock.Setup(s => s.CalculateFeesAsync(request, It.IsAny<CancellationToken>()))
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
                problemDetails?.Detail.Should().Be(ExceptionMessages.UnexpectedErrorCalculatingComplianceSchemeFees);
            }
        }
    }
}
