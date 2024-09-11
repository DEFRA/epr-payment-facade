using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
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
        public async Task GetBaseFeeAsync_ValidRequest_ReturnsOk(
            [Frozen] Mock<IComplianceSchemeFeesService> complianceSchemeFeesServiceMock,
            [Frozen] Mock<IValidator<RegulatorDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] RegulatorDto request,
            [Frozen] ComplianceSchemeBaseFeeResponse expectedResponse)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            complianceSchemeFeesServiceMock.Setup(s => s.GetComplianceSchemeBaseFeeAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetBaseFeeAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                var okResult = result as OkObjectResult;
                okResult?.Value.Should().BeEquivalentTo(expectedResponse);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetBaseFeeAsync_InvalidRequest_ReturnsBadRequest(
            [Frozen] Mock<IValidator<RegulatorDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] RegulatorDto request)
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Regulator", "Invalid regulator.")
            };
            var validationResult = new ValidationResult(validationFailures);
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            // Act
            var result = await controller.GetBaseFeeAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                var problemDetails = badRequestResult?.Value as ProblemDetails;

                problemDetails.Should().NotBeNull();
                problemDetails?.Title.Should().Be("Validation Error");
                problemDetails?.Detail.Should().Contain("Invalid regulator.");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetBaseFeeAsync_ServiceThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IComplianceSchemeFeesService> complianceSchemeFeesServiceMock,
            [Frozen] Mock<IValidator<RegulatorDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] RegulatorDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var validationException = new ValidationException("Validation error");
            complianceSchemeFeesServiceMock.Setup(s => s.GetComplianceSchemeBaseFeeAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act
            var result = await controller.GetBaseFeeAsync(request, CancellationToken.None);

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
        public async Task GetBaseFeeAsync_ServiceThrowsServiceException_ReturnsInternalServerError(
            [Frozen] Mock<IComplianceSchemeFeesService> complianceSchemeFeesServiceMock,
            [Frozen] Mock<IValidator<RegulatorDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] RegulatorDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var serviceException = new ServiceException(ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee);
            complianceSchemeFeesServiceMock.Setup(s => s.GetComplianceSchemeBaseFeeAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(serviceException);

            // Act
            var result = await controller.GetBaseFeeAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ObjectResult>();
                var objectResult = result as ObjectResult;
                var problemDetails = objectResult?.Value as ProblemDetails;

                objectResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
                problemDetails.Should().NotBeNull();
                problemDetails?.Title.Should().Be("Service Error");
                problemDetails?.Detail.Should().Be(ExceptionMessages.ErrorRetrievingComplianceSchemeBaseFee);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetBaseFeeAsync_ServiceThrowsException_ReturnsInternalServerError(
            [Frozen] Mock<IComplianceSchemeFeesService> complianceSchemeFeesServiceMock,
            [Frozen] Mock<IValidator<RegulatorDto>> validatorMock,
            [Greedy] ComplianceSchemeFeesController controller,
            [Frozen] RegulatorDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var exception = new Exception("Unexpected error");
            complianceSchemeFeesServiceMock.Setup(s => s.GetComplianceSchemeBaseFeeAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            var result = await controller.GetBaseFeeAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ObjectResult>();
                var objectResult = result as ObjectResult;
                var problemDetails = objectResult?.Value as ProblemDetails;

                objectResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
                problemDetails.Should().NotBeNull();
                problemDetails?.Title.Should().Be("Unexpected Error");
                problemDetails?.Detail.Should().Be(ExceptionMessages.UnexpectedErrorRetrievingComplianceSchemeBaseFee);
            }
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithValidArguments_ShouldInitializeCorrectly(
            [Frozen] Mock<IComplianceSchemeFeesService> complianceSchemeFeesServiceMock,
            [Frozen] Mock<ILogger<ComplianceSchemeFeesController>> loggerMock,
            [Frozen] Mock<IValidator<RegulatorDto>> complianceSchemeValidator)
        {
            // Act
            var controller = new ComplianceSchemeFeesController(
                complianceSchemeFeesServiceMock.Object,
                loggerMock.Object,
                complianceSchemeValidator.Object
            );

            // Assert
            controller.Should().NotBeNull();
            controller.Should().BeAssignableTo<ComplianceSchemeFeesController>();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullComplianceSchemeFeesService_ShouldThrowArgumentNullException(
            [Frozen] Mock<ILogger<ComplianceSchemeFeesController>> loggerMock,
            [Frozen] Mock<IValidator<RegulatorDto>> complianceSchemeValidator)
        {
            // Act
            Action act = () => new ComplianceSchemeFeesController(
                null!,
                loggerMock.Object,
                complianceSchemeValidator.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("complianceSchemeFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException(
            [Frozen] Mock<IComplianceSchemeFeesService> complianceSchemeFeesServiceMock,
            [Frozen] Mock<IValidator<RegulatorDto>> complianceSchemeValidator)
        {
            // Act
            Action act = () => new ComplianceSchemeFeesController(
                complianceSchemeFeesServiceMock.Object,
                null!,
                complianceSchemeValidator.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullComplianceSchemeValidator_ShouldThrowArgumentNullException(
            [Frozen] Mock<IComplianceSchemeFeesService> complianceSchemeFeesServiceMock,
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
                .WithParameterName("complianceSchemeValidator");
        }
    }
}
