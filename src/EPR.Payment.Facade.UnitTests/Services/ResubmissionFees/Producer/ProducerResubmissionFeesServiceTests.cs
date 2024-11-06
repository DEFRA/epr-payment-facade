using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.ResubmissionFees.Producer;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.RegistrationFees.Producer
{
    [TestClass]
    public class ProducerResubmissionFeesServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpProducerResubmissionFeesService> _httpProducerResubmissionFeesService = null!;
        private Mock<ILogger<ProducerResubmissionFeesService>> _loggerMock = null!;
        private ProducerResubmissionFeesService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _httpProducerResubmissionFeesService = _fixture.Freeze<Mock<IHttpProducerResubmissionFeesService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<ProducerResubmissionFeesService>>>();

            _service = new ProducerResubmissionFeesService(
                _httpProducerResubmissionFeesService.Object,
                _loggerMock.Object);
        }

        [TestMethod]
        public void Constructor_WithNullHttpProducerResubmissionFeesService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new ProducerResubmissionFeesService(null!, new Mock<ILogger<ProducerResubmissionFeesService>>().Object);

            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'httpProducerResubmissionFeesService')");
        }

        [TestMethod]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new ProducerResubmissionFeesService(new Mock<IHttpProducerResubmissionFeesService>().Object, null!);

            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'logger')");
        }


        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ReturnsAResult_ShouldReturnResponse(
            [Frozen] ProducerResubmissionFeeRequestDto request,
            [Frozen] ProducerResubmissionFeeResponseDto expectedResponse)
        {
            // Arrange
            _httpProducerResubmissionFeesService
                .Setup(i => i.GetResubmissionFeeAsync(request, CancellationToken.None))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.GetResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task GetResubmissionFeeAsync_RequestIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new ProducerResubmissionFeesService(
                new Mock<IHttpProducerResubmissionFeesService>().Object,
                new Mock<ILogger<ProducerResubmissionFeesService>>().Object);

            // Act & Assert
            await service.Invoking(async s => await s.GetResubmissionFeeAsync(null!, CancellationToken.None))
                .Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Error occurred while getting resubmission fee. (Parameter 'request')");
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ServiceThrowsException_ShouldLogAndThrow(
            [Frozen] Mock<IHttpProducerResubmissionFeesService> httpProducerResubmissionFeesService,
            [Frozen] ProducerResubmissionFeeRequestDto request,
            [Frozen] Mock<ILogger<ProducerResubmissionFeesService>> loggerMock)
        {
            // Arrange
            var exception = new Exception("Test Exception");
            httpProducerResubmissionFeesService
                .Setup(s => s.GetResubmissionFeeAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var service = new ProducerResubmissionFeesService(httpProducerResubmissionFeesService.Object, loggerMock.Object);

            // Act
            Func<Task> act = async () => await service.GetResubmissionFeeAsync(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Test Exception");

            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals("An unexpected error occurred while calculating the fees.", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

    }
}