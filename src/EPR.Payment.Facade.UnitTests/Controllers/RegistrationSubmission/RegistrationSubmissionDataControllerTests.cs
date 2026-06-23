using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.RegistrationSubmission;
using EPR.Payment.Facade.Services.RegistrationSubmission.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers.RegistrationSubmission
{
    [TestClass]
    public class RegistrationSubmissionDataControllerTests
    {
        [TestMethod, AutoMoqData]
        public async Task GetFeeCalculationDetails_Valid_ReturnsOkWithList(
            [Frozen] Mock<IRegistrationSubmissionDataService> serviceMock,
            [Greedy] RegistrationSubmissionDataController controller,
            Guid submissionId)
        {
            IReadOnlyList<RegistrationFeeCalculationDetailsDto> expected = new[]
            {
                new RegistrationFeeCalculationDetailsDto { OrganisationId = "ORG-1" },
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
                .ReturnsAsync((IReadOnlyList<RegistrationFeeCalculationDetailsDto>?)null);

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
