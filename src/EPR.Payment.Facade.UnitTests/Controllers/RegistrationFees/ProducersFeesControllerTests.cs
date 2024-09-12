﻿using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.RegistrationFees;
using EPR.Payment.Facade.Services.RegistrationFees.Producer.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers
{
    [TestClass]
    public class ProducersFeesControllerTests
    {
        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ValidRequest_ReturnsOk(
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] Mock<IValidator<ProducerFeesRequestDto>> validatorMock,
            [Greedy] ProducersFeesController controller,
            [Frozen] ProducerFeesRequestDto request,
            [Frozen] ProducerFeesResponseDto expectedResponse)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerFeesServiceMock.Setup(s => s.CalculateProducerFeesAsync(request, It.IsAny<CancellationToken>()))
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
            [Frozen] Mock<IValidator<ProducerFeesRequestDto>> validatorMock,
            [Greedy] ProducersFeesController controller,
            [Frozen] ProducerFeesRequestDto request)
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("ProducerType", ValidationMessages.ProducerTypeInvalid)
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
                problemDetails?.Detail.Should().Contain(ValidationMessages.ProducerTypeInvalid);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ServiceThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] Mock<IValidator<ProducerFeesRequestDto>> validatorMock,
            [Greedy] ProducersFeesController controller,
            [Frozen] ProducerFeesRequestDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var validationException = new ValidationException("Validation error");
            producerFeesServiceMock.Setup(s => s.CalculateProducerFeesAsync(request, It.IsAny<CancellationToken>()))
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
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] Mock<IValidator<ProducerFeesRequestDto>> validatorMock,
            [Greedy] ProducersFeesController controller,
            [Frozen] ProducerFeesRequestDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var serviceException = new ServiceException(ExceptionMessages.ErrorCalculatingProducerFees);
            producerFeesServiceMock.Setup(s => s.CalculateProducerFeesAsync(request, It.IsAny<CancellationToken>()))
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
                problemDetails?.Detail.Should().Be(ExceptionMessages.ErrorCalculatingProducerFees);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateFeesAsync_ServiceThrowsException_ReturnsInternalServerError(
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] Mock<IValidator<ProducerFeesRequestDto>> validatorMock,
            [Greedy] ProducersFeesController controller,
            [Frozen] ProducerFeesRequestDto request)
        {
            // Arrange
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var exception = new Exception("Unexpected error");
            producerFeesServiceMock.Setup(s => s.CalculateProducerFeesAsync(request, It.IsAny<CancellationToken>()))
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
                problemDetails?.Detail.Should().Be(ExceptionMessages.UnexpectedErrorCalculatingFees);
            }
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithValidArguments_ShouldInitializeCorrectly(
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] Mock<ILogger<ProducersFeesController>> loggerMock,
            [Frozen] Mock<IValidator<ProducerFeesRequestDto>> registrationValidator,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidator)
        {
            // Act
            var controller = new ProducersFeesController(
                producerFeesServiceMock.Object,
                loggerMock.Object,
                registrationValidator.Object,
                resubmissionValidator.Object
            );

            // Assert
            controller.Should().NotBeNull();
            controller.Should().BeAssignableTo<ProducersFeesController>();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullRegistrationFeesService_ShouldThrowArgumentNullException(
            [Frozen] Mock<ILogger<ProducersFeesController>> loggerMock,
            [Frozen] Mock<IValidator<ProducerFeesRequestDto>> registrationValidator,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidator)
        {
            // Act
            Action act = () => new ProducersFeesController(
                null!,
                loggerMock.Object,
                registrationValidator.Object,
                resubmissionValidator.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("producerFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException(
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] Mock<IValidator<ProducerFeesRequestDto>> registrationValidator,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidator)
        {
            // Act
            Action act = () => new ProducersFeesController(
                producerFeesServiceMock.Object,
                null!,
                registrationValidator.Object,
                resubmissionValidator.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullRegistrationValidator_ShouldThrowArgumentNullException(
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] Mock<ILogger<ProducersFeesController>> loggerMock,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidator)
        {
            // Act
            Action act = () => new ProducersFeesController(
                producerFeesServiceMock.Object,
                loggerMock.Object,
                null!,
                resubmissionValidator.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("registrationValidator");
        }


        [TestMethod, AutoMoqData]
        public void Constructor_WithNullResubmissionValidator_ShouldThrowArgumentNullException(
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] Mock<ILogger<ProducersFeesController>> loggerMock,
            [Frozen] Mock<IValidator<ProducerFeesRequestDto>> registrationValidatorMock)
        {
            // Act
            Action act = () => new ProducersFeesController(
                producerFeesServiceMock.Object,
                loggerMock.Object,
                registrationValidatorMock.Object,
                null!
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("resubmissionValidator");
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ServiceReturnsAResult_ShouldReturnOkResponse(
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] RegulatorDto request,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock,
            [Frozen] decimal expectedAmount,
            [Greedy] ProducersFeesController controller)
        {
            //Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerFeesServiceMock.Setup(i => i.GetResubmissionFeeAsync(request, CancellationToken.None)).ReturnsAsync(expectedAmount);

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
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] RegulatorDto request,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock,
            [Greedy] ProducersFeesController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerFeesServiceMock.Setup(i => i.GetResubmissionFeeAsync(request, CancellationToken.None))
                               .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await controller.GetResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionAsync_ThrowsValidationException_ShouldReturnBadRequest(
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] RegulatorDto request,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock,
            [Greedy] ProducersFeesController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);
            producerFeesServiceMock.Setup(s => s.GetResubmissionFeeAsync(It.IsAny<RegulatorDto>(), CancellationToken.None)).ThrowsAsync(new ValidationException("Validation error"));

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
            [Frozen] Mock<IProducerFeesService> producerFeesServiceMock,
            [Frozen] RegulatorDto request,
            [Frozen] Mock<IValidator<RegulatorDto>> resubmissionValidatorMock,
            [Greedy] ProducersFeesController controller)
        {
            // Arrange
            var validationResult = new ValidationResult();
            resubmissionValidatorMock.Setup(v => v.Validate(request)).Returns(validationResult);

            var validationException = new ValidationException("Validation error");
            producerFeesServiceMock.Setup(s => s.GetResubmissionFeeAsync(request, It.IsAny<CancellationToken>()))
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
            [Greedy] ProducersFeesController controller)
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