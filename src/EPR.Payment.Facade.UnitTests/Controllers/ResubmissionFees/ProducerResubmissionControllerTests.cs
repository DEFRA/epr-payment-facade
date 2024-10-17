using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
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
            [Frozen] Mock<ILogger<ProducerResubmissionController>> loggerMock,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock)
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
            [Frozen] RegulatorDto request,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock,
            [Frozen] decimal expectedAmount,
            [Greedy] ProducerResubmissionController controller)
        {
            //Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerResubmissionFeesService.Setup(i => i.GetResubmissionFeeAsync(request, CancellationToken.None)).ReturnsAsync(expectedAmount);

            //Act
            var result = await controller.GetResubmissionFeeAsync(request, CancellationToken.None);

            //Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                result.As<OkObjectResult>().Should().NotBeNull();
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ServiceThrowsException_ShouldReturnInternalServerError(
            [Frozen] Mock<IProducerResubmissionFeesService> producerResubmissionFeesService,
            [Frozen] RegulatorDto request,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock,
            [Greedy] ProducerResubmissionController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerResubmissionFeesService.Setup(i => i.GetResubmissionFeeAsync(request, CancellationToken.None))
                               .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await controller.GetResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionAsync_ThrowsValidationException_ShouldReturnBadRequest(
            [Frozen] Mock<IProducerResubmissionFeesService> producerResubmissionFeesService,
            [Frozen] RegulatorDto request,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock,
            [Greedy] ProducerResubmissionController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerResubmissionFeesService.Setup(s => s.GetResubmissionFeeAsync(It.IsAny<RegulatorDto>(), CancellationToken.None)).ThrowsAsync(new ValidationException("Validation error"));

            // Act
            var result = await controller.GetResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();

                var badRequestResult = result as BadRequestObjectResult;
                badRequestResult.Should().NotBeNull();
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionAsync_ServiceThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IProducerResubmissionFeesService> producerResubmissionFeesService,
            [Frozen] RegulatorDto request,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock,
            [Greedy] ProducerResubmissionController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var validationException = new ValidationException("Validation error");
            producerResubmissionFeesService.Setup(s => s.GetResubmissionFeeAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

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
                problemDetails?.Detail.Should().Be(validationException.Message);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionAsync_InvalidRequest_ReturnsBadRequest(
            [Frozen] RegulatorDto request,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock,
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
