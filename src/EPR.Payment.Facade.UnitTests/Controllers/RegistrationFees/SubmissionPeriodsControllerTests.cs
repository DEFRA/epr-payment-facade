using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Controllers.RegistrationFees;
using EPR.Payment.Facade.Services.RegistrationFees.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Controllers.RegistrationFees
{
    [TestClass]
    public class SubmissionPeriodsControllerTests
    {
        [TestMethod, AutoMoqData]
        public void Constructor_NullService_Throws(ILogger<SubmissionPeriodsController> logger)
        {
            Action act = () => new SubmissionPeriodsController(null!, logger);
            act.Should().Throw<ArgumentNullException>().WithParameterName("submissionPeriodsService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_NullLogger_Throws(ISubmissionPeriodsService service)
        {
            Action act = () => new SubmissionPeriodsController(service, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task GetSubmissionPeriods_ReturnsOkWithPayload(
            Mock<ISubmissionPeriodsService> serviceMock,
            ILogger<SubmissionPeriodsController> logger)
        {
            IReadOnlyList<SubmissionPeriodResponseDto> rows = new[]
            {
                new SubmissionPeriodResponseDto { Id = 1, WindowType = "Cso", RegistrationYear = 2025 },
            };
            serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(rows);
            var sut = new SubmissionPeriodsController(serviceMock.Object, logger);

            var result = await sut.GetSubmissionPeriods(CancellationToken.None);

            using (new AssertionScope())
            {
                var ok = result.Should().BeOfType<OkObjectResult>().Which;
                ok.Value.Should().BeEquivalentTo(rows);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task GetSubmissionPeriods_ServiceException_Returns500(
            Mock<ISubmissionPeriodsService> serviceMock,
            ILogger<SubmissionPeriodsController> logger)
        {
            serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new ServiceException("boom"));
            var sut = new SubmissionPeriodsController(serviceMock.Object, logger);

            var result = await sut.GetSubmissionPeriods(CancellationToken.None);

            using (new AssertionScope())
            {
                var objectResult = result.Should().BeOfType<ObjectResult>().Which;
                objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
                objectResult.Value.Should().BeOfType<ProblemDetails>();
            }
        }
    }
}
