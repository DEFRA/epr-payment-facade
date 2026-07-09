using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.RegistrationFees;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.RegistrationFees
{
    [TestClass]
    public class SubmissionPeriodsServiceTests
    {
        [TestMethod, AutoMoqData]
        public void Constructor_NullHttpService_Throws(ILogger<SubmissionPeriodsService> logger)
        {
            Action act = () => new SubmissionPeriodsService(null!, logger);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_NullLogger_Throws(IHttpSubmissionPeriodsService http)
        {
            Action act = () => new SubmissionPeriodsService(http, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task GetAllAsync_ReturnsListFromHttp(
            Mock<IHttpSubmissionPeriodsService> httpMock,
            ILogger<SubmissionPeriodsService> logger)
        {
            IReadOnlyList<SubmissionPeriodResponseDto> expected = new[]
            {
                new SubmissionPeriodResponseDto { Id = 1, WindowType = "Cso", RegistrationYear = 2025 },
            };
            httpMock.Setup(h => h.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);
            var sut = new SubmissionPeriodsService(httpMock.Object, logger);

            var result = await sut.GetAllAsync(CancellationToken.None);

            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod, AutoMoqData]
        public async Task GetAllAsync_HttpThrows_WrapsInServiceException(
            Mock<IHttpSubmissionPeriodsService> httpMock,
            ILogger<SubmissionPeriodsService> logger)
        {
            httpMock.Setup(h => h.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
            var sut = new SubmissionPeriodsService(httpMock.Object, logger);

            Func<Task> act = () => sut.GetAllAsync(CancellationToken.None);

            await act.Should().ThrowAsync<ServiceException>();
        }
    }
}
