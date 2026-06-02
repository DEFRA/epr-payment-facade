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

        [TestMethod, AutoMoqData]
        public async Task GetFeeCalculationDetails_Valid_ReturnsOkWithList(
            [Frozen] Mock<IRegistrationSubmissionDataService> serviceMock,
            [Greedy] RegistrationSubmissionDataController controller,
            Guid submissionId)
        {
            IReadOnlyList<EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission.RegistrationFeeCalculationDetailsDto> expected = new[]
            {
                new EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission.RegistrationFeeCalculationDetailsDto { OrganisationId = "ORG-1" },
            };
            serviceMock.Setup(s => s.GetFeeCalculationDetailsAsync(submissionId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

            var result = await controller.GetFeeCalculationDetails(submissionId, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expected);
        }

        [TestMethod, AutoMoqData]
        public async Task GetFeeCalculationDetails_Null_ReturnsNotFound(
            [Frozen] Mock<IRegistrationSubmissionDataService> serviceMock,
            [Greedy] RegistrationSubmissionDataController controller,
            Guid submissionId)
        {
            serviceMock.Setup(s => s.GetFeeCalculationDetailsAsync(submissionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission.RegistrationFeeCalculationDetailsDto>?)null);

            var result = await controller.GetFeeCalculationDetails(submissionId, CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod, AutoMoqData]
        public async Task GetFeeCalculationDetails_EmptyGuid_ReturnsBadRequest(
            [Frozen] Mock<IRegistrationSubmissionDataService> serviceMock,
            [Greedy] RegistrationSubmissionDataController controller)
        {
            var result = await controller.GetFeeCalculationDetails(Guid.Empty, CancellationToken.None);

            using (new AssertionScope())
            {
                result.Should().BeOfType<BadRequestObjectResult>();
                serviceMock.Verify(s => s.GetFeeCalculationDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetFeeCalculationDetails_ServiceException_Returns500(
            [Frozen] Mock<IRegistrationSubmissionDataService> serviceMock,
            [Greedy] RegistrationSubmissionDataController controller,
            Guid submissionId)
        {
            serviceMock.Setup(s => s.GetFeeCalculationDetailsAsync(submissionId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ServiceException("downstream"));

            var result = await controller.GetFeeCalculationDetails(submissionId, CancellationToken.None);

            var status = result.Should().BeOfType<ObjectResult>().Which;
            status.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}
