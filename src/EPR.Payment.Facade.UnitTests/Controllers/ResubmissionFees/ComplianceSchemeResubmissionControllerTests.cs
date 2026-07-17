using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers.ResubmissionFees
{
    [TestClass]
    public class ComplianceSchemeResubmissionControllerTests
    {
        [TestMethod, AutoMoqData]
        public void Constructor_WithNullResubmissionFeesService_ShouldThrowArgumentNullException(
                    [Frozen] Mock<ILogger<ComplianceSchemeResubmissionController>> loggerMock,
                    [Frozen] Mock<IValidator<ComplianceSchemeResubmissionFeeRequestDto>> resubmissionValidatorMock)
        {
            // Act
            Action act = () => new ComplianceSchemeResubmissionController(
                null!,
                loggerMock.Object,
                resubmissionValidatorMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("resubmissionFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException(
            [Frozen] Mock<IComplianceSchemeResubmissionFeesService> resubmissionFeesServiceMock,
            [Frozen] Mock<IValidator<ComplianceSchemeResubmissionFeeRequestDto>> resubmissionValidatorMock)
        {
            // Act
            Action act = () => new ComplianceSchemeResubmissionController(
                resubmissionFeesServiceMock.Object,
                null!,
                resubmissionValidatorMock.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullResubmissionValidator_ShouldThrowArgumentNullException(
            [Frozen] Mock<IComplianceSchemeResubmissionFeesService> resubmissionFeesServiceMock,
            [Frozen] Mock<ILogger<ComplianceSchemeResubmissionController>> loggerMock)
        {
            // Act
            Action act = () => new ComplianceSchemeResubmissionController(
                resubmissionFeesServiceMock.Object,
                loggerMock.Object,
                null!
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("resubmissionValidator");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_ServiceReturnsAResult_ShouldReturnOkResponse(
            [Frozen] Mock<IComplianceSchemeResubmissionFeesService> resubmissionFeesService,
            [Frozen] ComplianceSchemeResubmissionFeeRequestDto request,
            [Frozen] Mock<IValidator<ComplianceSchemeResubmissionFeeRequestDto>> resubmissionValidatorMock,
            [Frozen] ComplianceSchemeResubmissionFeeResponse expectedResult,
            [Greedy] ComplianceSchemeResubmissionController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.ValidateAsync(request, CancellationToken.None)).ReturnsAsync(validationResult);
            resubmissionFeesService.Setup(i => i.CalculateResubmissionFeeAsync(request, CancellationToken.None)).ReturnsAsync(expectedResult);

            // Act
            var result = await controller.CalculateResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                result.As<OkObjectResult>().Value.Should().BeEquivalentTo(expectedResult);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_ServiceThrowsException_ShouldReturnInternalServerError(
            [Frozen] Mock<IComplianceSchemeResubmissionFeesService> resubmissionFeesService,
            [Frozen] ComplianceSchemeResubmissionFeeRequestDto request,
            [Frozen] Mock<IValidator<ComplianceSchemeResubmissionFeeRequestDto>> resubmissionValidatorMock,
            [Greedy] ComplianceSchemeResubmissionController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.ValidateAsync(request, CancellationToken.None)).ReturnsAsync(validationResult);
            resubmissionFeesService.Setup(i => i.CalculateResubmissionFeeAsync(request, CancellationToken.None))
                                   .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await controller.CalculateResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_ServiceThrowsValidationException_ShouldReturnBadRequest(
            [Frozen] Mock<IComplianceSchemeResubmissionFeesService> resubmissionFeesService,
            [Frozen] ComplianceSchemeResubmissionFeeRequestDto request,
            [Frozen] Mock<IValidator<ComplianceSchemeResubmissionFeeRequestDto>> resubmissionValidatorMock,
            [Greedy] ComplianceSchemeResubmissionController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.ValidateAsync(request, CancellationToken.None)).ReturnsAsync(validationResult);
            resubmissionFeesService.Setup(s => s.CalculateResubmissionFeeAsync(It.IsAny<ComplianceSchemeResubmissionFeeRequestDto>(), CancellationToken.None))
                                   .ThrowsAsync(new ValidationException("Validation error"));

            // Act
            var result = await controller.CalculateResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                var problemDetails = badRequestResult?.Value as ProblemDetails;

                problemDetails.Should().NotBeNull();
                problemDetails?.Title.Should().Be("Validation Error");
                problemDetails?.Detail.Should().Be("Validation error");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateResubmissionFeeAsync_InvalidRequest_ReturnsBadRequest(
            [Frozen] ComplianceSchemeResubmissionFeeRequestDto request,
            [Frozen] Mock<IValidator<ComplianceSchemeResubmissionFeeRequestDto>> resubmissionValidatorMock,
            [Greedy] ComplianceSchemeResubmissionController controller)
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Regulator", "Invalid regulator parameter.")
            };
            var validationResult = new ValidationResult(validationFailures);
            resubmissionValidatorMock.Setup(v => v.ValidateAsync(request, CancellationToken.None)).ReturnsAsync(validationResult);

            // Act
            var result = await controller.CalculateResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                var badRequestResult = result as BadRequestObjectResult;
                var problemDetails = badRequestResult?.Value as ProblemDetails;

                problemDetails.Should().NotBeNull();
                problemDetails?.Title.Should().Be("Validation Error");
                problemDetails?.Detail.Should().Contain("Invalid regulator parameter.");
            }
        }
    }
}