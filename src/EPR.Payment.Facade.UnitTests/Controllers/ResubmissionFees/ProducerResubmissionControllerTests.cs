using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.ResubmissionFees.Producer;
using EPR.Payment.Facade.Services.ResubmissionFees.Producer.Interfaces;
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
    public class ProducerResubmissionControllerTests
    {
        [TestMethod, AutoMoqData]
        public void Constructor_WithNullResubmissionValidator_ShouldThrowArgumentNullException(
            [Frozen] Mock<IProducerResubmissionFeesService> producerResubmissionFeesServiceMock,
            [Frozen] Mock<ILogger<ProducerResubmissionController>> loggerMock)
        {
            // Act
            Action act = () => new ProducerResubmissionController(
                producerResubmissionFeesServiceMock.Object,
                loggerMock.Object,
                null!
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("resubmissionValidator");
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ServiceReturnsAResult_ShouldReturnOkResponse(
            [Frozen] Mock<IProducerResubmissionFeesService> producerResubmissionFeesService,
            [Frozen] ProducerResubmissionFeeRequestDto request,
            [Frozen] Mock<IValidator<ProducerResubmissionFeeRequestDto>> resubmissionValidatorMock,
            [Frozen] ProducerResubmissionFeeResponseDto expectedResponse,
            [Greedy] ProducerResubmissionController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerResubmissionFeesService.Setup(s => s.GetResubmissionFeeAsync(request, CancellationToken.None))
                                           .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                var okResult = result as OkObjectResult;
                okResult?.Value.Should().BeEquivalentTo(expectedResponse);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ServiceThrowsException_ShouldReturnInternalServerError(
            [Frozen] Mock<IProducerResubmissionFeesService> producerResubmissionFeesService,
            [Frozen] ProducerResubmissionFeeRequestDto request,
            [Frozen] Mock<IValidator<ProducerResubmissionFeeRequestDto>> resubmissionValidatorMock,
            [Greedy] ProducerResubmissionController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerResubmissionFeesService.Setup(s => s.GetResubmissionFeeAsync(request, CancellationToken.None))
                                           .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await controller.GetResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ThrowsValidationException_ShouldReturnBadRequest(
            [Frozen] Mock<IProducerResubmissionFeesService> producerResubmissionFeesService,
            [Frozen] ProducerResubmissionFeeRequestDto request,
            [Frozen] Mock<IValidator<ProducerResubmissionFeeRequestDto>> resubmissionValidatorMock,
            [Greedy] ProducerResubmissionController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerResubmissionFeesService.Setup(s => s.GetResubmissionFeeAsync(request, CancellationToken.None))
                                           .ThrowsAsync(new ValidationException("Validation error"));

            // Act
            var result = await controller.GetResubmissionFeeAsync(request, CancellationToken.None);

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
        public async Task GetResubmissionFeeAsync_InvalidRequest_ShouldReturnBadRequest(
            [Frozen] ProducerResubmissionFeeRequestDto request,
            [Frozen] Mock<IValidator<ProducerResubmissionFeeRequestDto>> resubmissionValidatorMock,
            [Greedy] ProducerResubmissionController controller)
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Regulator", "Invalid regulator parameter.")
            };
            var validationResult = new ValidationResult(validationFailures);
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            // Act
            var result = await controller.GetResubmissionFeeAsync(request, CancellationToken.None);

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