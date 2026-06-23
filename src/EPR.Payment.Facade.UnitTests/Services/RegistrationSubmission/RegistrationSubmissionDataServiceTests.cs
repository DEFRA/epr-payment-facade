using EPR.Payment.Facade.Common.Dtos.Response.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.RegistrationSubmission;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.RegistrationSubmission
{
    [TestClass]
    public class RegistrationSubmissionDataServiceTests
    {
        [TestMethod, AutoMoqData]
        public void Constructor_NullHttpService_Throws(ILogger<RegistrationSubmissionDataService> logger)
        {
            Action act = () => new RegistrationSubmissionDataService(null!, logger);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_NullLogger_Throws(IHttpRegistrationSubmissionDataService http)
        {
            Action act = () => new RegistrationSubmissionDataService(http, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task GetFeeCalculationDetailsAsync_ReturnsListFromHttp(
            Mock<IHttpRegistrationSubmissionDataService> httpMock,
            ILogger<RegistrationSubmissionDataService> logger,
            Guid submissionId)
        {
            IReadOnlyList<RegistrationFeeCalculationDetailsDto> expected = new[]
            {
                new RegistrationFeeCalculationDetailsDto { OrganisationId = "ORG-1" },
            };
            httpMock.Setup(h => h.GetFeeCalculationDetailsAsync(submissionId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
            var sut = new RegistrationSubmissionDataService(httpMock.Object, logger);

            var result = await sut.GetFeeCalculationDetailsAsync(submissionId);

            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod, AutoMoqData]
        public async Task GetFeeCalculationDetailsAsync_HttpReturnsNull_ReturnsNull(
            Mock<IHttpRegistrationSubmissionDataService> httpMock,
            ILogger<RegistrationSubmissionDataService> logger,
            Guid submissionId)
        {
            httpMock.Setup(h => h.GetFeeCalculationDetailsAsync(submissionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<RegistrationFeeCalculationDetailsDto>?)null);
            var sut = new RegistrationSubmissionDataService(httpMock.Object, logger);

            var result = await sut.GetFeeCalculationDetailsAsync(submissionId);

            result.Should().BeNull();
        }

        [TestMethod, AutoMoqData]
        public async Task GetFeeCalculationDetailsAsync_HttpThrows_WrapsInServiceException(
            Mock<IHttpRegistrationSubmissionDataService> httpMock,
            ILogger<RegistrationSubmissionDataService> logger,
            Guid submissionId)
        {
            httpMock.Setup(h => h.GetFeeCalculationDetailsAsync(submissionId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
            var sut = new RegistrationSubmissionDataService(httpMock.Object, logger);

            Func<Task> act = () => sut.GetFeeCalculationDetailsAsync(submissionId);

            await act.Should().ThrowAsync<ServiceException>();
        }
    }
}
