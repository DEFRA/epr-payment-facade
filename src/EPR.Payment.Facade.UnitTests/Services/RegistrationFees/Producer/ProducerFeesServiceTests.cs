using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.RegistrationFees.Producer;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.RegistrationFees.Producer
{
    [TestClass]
    public class ProducerFeesServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpProducerFeesService> _httpProducerFeesService = null!;
        private Mock<ILogger<ProducerFeesService>> _loggerMock = null!;
        private ProducerFeesService _service = null!;


        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _httpProducerFeesService = _fixture.Freeze<Mock<IHttpProducerFeesService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<ProducerFeesService>>>();

            _service = new ProducerFeesService(
                _httpProducerFeesService.Object,
                _loggerMock.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpRegistrationFeesServiceIsNull_ShouldThrowArgumentNullException(
            ILogger<ProducerFeesService> logger)
        {
            // Act
            Action act = () => new ProducerFeesService(null!, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpProducerFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_LoggerIsNull_ShouldThrowArgumentNullException(
            IHttpProducerFeesService httpProducerFeesService)
        {
            // Act
            Action act = () => new ProducerFeesService(httpProducerFeesService, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_RequestIsNull_ShouldThrowArgumentNullException(
            ProducerFeesService service)
        {
            // Act
            Func<Task> act = () => service.CalculateProducerFeesAsync((ProducerFeesRequestDto)null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.ErrorCalculatingProducerFees} (Parameter 'request')");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_RequestIsValid_ShouldReturnResponse(
            ProducerFeesResponseDto expectedResponse,
            ProducerFeesRequestDto request)
        {
            // Arrange
            _httpProducerFeesService.Setup(s => s.CalculateProducerFeesAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.CalculateProducerFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_HttpServiceThrowsException_ShouldLogAndThrowServiceException(
            ProducerFeesRequestDto request)
        {
            // Arrange
            var exceptionMessage = "Unexpected error occurred";
            var exception = new Exception(exceptionMessage);

            _httpProducerFeesService.Setup(s => s.CalculateProducerFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            Func<Task> act = async () => await _service.CalculateProducerFeesAsync(request);

            // Assert
            var thrownException = await act.Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorCalculatingProducerFees);

            thrownException.Which.InnerException.Should().BeOfType<Exception>()
                .Which.Message.Should().Be(exceptionMessage);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.UnexpectedErrorCalculatingProducerFees)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_HttpServiceThrowsException_ShouldLogAndThrowValidationException(
            ProducerFeesRequestDto request)
        {
            // Arrange
            var exceptionMessage = "Validation error";
            var validationException = new ValidationException(exceptionMessage);

            _httpProducerFeesService.Setup(s => s.CalculateProducerFeesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            // Act
            Func<Task> act = async () => await _service.CalculateProducerFeesAsync(request);

            // Assert
            using (new AssertionScope())
            {
                var thrownException = await act.Should().ThrowAsync<ValidationException>();

                thrownException.Which.Message.Should().Be(exceptionMessage);
            }
        }

        [TestMethod]
        public async Task GetFeesBySubmissionAsync_HttpServiceReturnsResponse_PassesThrough()
        {
            var submissionId = Guid.NewGuid();
            var expected = _fixture.Create<ProducerFeesResponseDto>();
            _httpProducerFeesService
                .Setup(h => h.GetFeesBySubmissionAsync(submissionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _service.GetFeesBySubmissionAsync(submissionId, CancellationToken.None);

            result.Should().BeSameAs(expected);
        }

        [TestMethod]
        public async Task GetFeesBySubmissionAsync_HttpServiceReturnsNull_ReturnsNull()
        {
            _httpProducerFeesService
                .Setup(h => h.GetFeesBySubmissionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProducerFeesResponseDto?)null);

            var result = await _service.GetFeesBySubmissionAsync(Guid.NewGuid(), CancellationToken.None);

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetFeesBySubmissionAsync_HttpServiceThrowsServiceException_Rethrows()
        {
            var inner = new ServiceException("inner");
            _httpProducerFeesService
                .Setup(h => h.GetFeesBySubmissionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(inner);

            Func<Task> act = async () => await _service.GetFeesBySubmissionAsync(Guid.NewGuid(), CancellationToken.None);

            var thrown = await act.Should().ThrowAsync<ServiceException>();
            thrown.Which.Should().BeSameAs(inner);
        }

        [TestMethod]
        public async Task GetFeesBySubmissionAsync_HttpServiceThrowsUnexpected_WrapsInServiceException()
        {
            _httpProducerFeesService
                .Setup(h => h.GetFeesBySubmissionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("oops"));

            Func<Task> act = async () => await _service.GetFeesBySubmissionAsync(Guid.NewGuid(), CancellationToken.None);

            await act.Should().ThrowAsync<ServiceException>()
                .WithMessage(ExceptionMessages.ErrorCalculatingProducerFees);
        }
    }
}
