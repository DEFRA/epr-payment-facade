using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.RegistrationSubmission;
using EPR.Payment.Facade.Services.RegistrationSubmission.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers.RegistrationSubmission
{
    [TestClass]
    public class RegistrationSubmissionDataControllerTests
    {
        [TestMethod, AutoMoqData]
        public async Task CreateAsync_Valid_ReturnsOkWithId(
            [Frozen] Mock<IRegistrationSubmissionDataService> serviceMock,
            [Frozen] Mock<IValidator<CreateRegistrationSubmissionDataRequest>> validatorMock,
            [Greedy] RegistrationSubmissionDataController controller,
            CreateRegistrationSubmissionDataRequest request,
            Guid expected)
        {
            validatorMock.Setup(v => v.Validate(request)).Returns(new ValidationResult());
            serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

            var result = await controller.CreateAsync(request, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(expected);
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_ValidationFails_ReturnsBadRequest(
            [Frozen] Mock<IValidator<CreateRegistrationSubmissionDataRequest>> validatorMock,
            [Greedy] RegistrationSubmissionDataController controller,
            CreateRegistrationSubmissionDataRequest request)
        {
            validatorMock.Setup(v => v.Validate(request)).Returns(new ValidationResult(new[]
            {
                new ValidationFailure("SubmissionId", "SubmissionId is required."),
            }));

            var result = await controller.CreateAsync(request, CancellationToken.None);

            using (new AssertionScope())
            {
                var bad = result.Should().BeOfType<BadRequestObjectResult>().Which;
                bad.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
                bad.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("SubmissionId is required");
            }
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_ServiceThrowsValidationException_ReturnsBadRequest(
            [Frozen] Mock<IRegistrationSubmissionDataService> serviceMock,
            [Frozen] Mock<IValidator<CreateRegistrationSubmissionDataRequest>> validatorMock,
            [Greedy] RegistrationSubmissionDataController controller,
            CreateRegistrationSubmissionDataRequest request)
        {
            validatorMock.Setup(v => v.Validate(request)).Returns(new ValidationResult());
            serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("downstream rejected"));

            var result = await controller.CreateAsync(request, CancellationToken.None);

            var bad = result.Should().BeOfType<BadRequestObjectResult>().Which;
            bad.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Be("downstream rejected");
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_ServiceThrowsServiceException_Returns500(
            [Frozen] Mock<IRegistrationSubmissionDataService> serviceMock,
            [Frozen] Mock<IValidator<CreateRegistrationSubmissionDataRequest>> validatorMock,
            [Greedy] RegistrationSubmissionDataController controller,
            CreateRegistrationSubmissionDataRequest request)
        {
            validatorMock.Setup(v => v.Validate(request)).Returns(new ValidationResult());
            serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ServiceException("payment-service down"));

            var result = await controller.CreateAsync(request, CancellationToken.None);

            var status = result.Should().BeOfType<ObjectResult>().Which;
            status.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_ServiceThrowsUnknown_Returns500(
            [Frozen] Mock<IRegistrationSubmissionDataService> serviceMock,
            [Frozen] Mock<IValidator<CreateRegistrationSubmissionDataRequest>> validatorMock,
            [Greedy] RegistrationSubmissionDataController controller,
            CreateRegistrationSubmissionDataRequest request)
        {
            validatorMock.Setup(v => v.Validate(request)).Returns(new ValidationResult());
            serviceMock.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("kaboom"));

            var result = await controller.CreateAsync(request, CancellationToken.None);

            var status = result.Should().BeOfType<ObjectResult>().Which;
            status.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}
