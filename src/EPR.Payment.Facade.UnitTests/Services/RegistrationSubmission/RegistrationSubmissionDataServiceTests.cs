using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationSubmission;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationSubmission.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.RegistrationSubmission;
using FluentAssertions;
using FluentValidation;
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
        public async Task CreateAsync_NullRequest_Throws(RegistrationSubmissionDataService sut)
        {
            Func<Task> act = () => sut.CreateAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.ErrorCreatingRegistrationSubmissionData} (Parameter 'request')");
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_HttpReturnsId_ReturnsId(
            Mock<IHttpRegistrationSubmissionDataService> httpMock,
            ILogger<RegistrationSubmissionDataService> logger,
            CreateRegistrationSubmissionDataRequest request,
            Guid expectedId)
        {
            httpMock.Setup(h => h.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(expectedId);
            var sut = new RegistrationSubmissionDataService(httpMock.Object, logger);

            var id = await sut.CreateAsync(request);

            id.Should().Be(expectedId);
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_HttpThrowsValidationException_Rethrows(
            Mock<IHttpRegistrationSubmissionDataService> httpMock,
            ILogger<RegistrationSubmissionDataService> logger,
            CreateRegistrationSubmissionDataRequest request)
        {
            httpMock.Setup(h => h.CreateAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("invalid"));
            var sut = new RegistrationSubmissionDataService(httpMock.Object, logger);

            Func<Task> act = () => sut.CreateAsync(request);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*invalid*");
        }

        [TestMethod, AutoMoqData]
        public async Task CreateAsync_HttpThrowsUnknown_WrapsInServiceException(
            Mock<IHttpRegistrationSubmissionDataService> httpMock,
            ILogger<RegistrationSubmissionDataService> logger,
            CreateRegistrationSubmissionDataRequest request)
        {
            httpMock.Setup(h => h.CreateAsync(request, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
            var sut = new RegistrationSubmissionDataService(httpMock.Object, logger);

            Func<Task> act = () => sut.CreateAsync(request);

            await act.Should().ThrowAsync<ServiceException>();
        }
    }
}
