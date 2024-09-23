using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees;
using EPR.Payment.Facade.Common.Exceptions;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Extension;
using EPR.Payment.Facade.Services.RegistrationFees;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace EPR.Payment.Facade.UnitTests.Services.RegistrationFees
{
    [TestClass]
    public class RegistrationFeesServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IHttpRegistrationFeesService> _httpRegistrationFeesServiceMock = null!;
        private Mock<ILogger<RegistrationFeesService>> _loggerMock = null!;
        private RegistrationFeesService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _httpRegistrationFeesServiceMock = _fixture.Freeze<Mock<IHttpRegistrationFeesService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<RegistrationFeesService>>>();

            _service = new RegistrationFeesService(
                _httpRegistrationFeesServiceMock.Object,
                _loggerMock.Object);
        }

        [TestMethod, AutoMoqData]
        public void Constructor_HttpRegistrationFeesServiceIsNull_ShouldThrowArgumentNullException(
            ILogger<RegistrationFeesService> logger)
        {
            // Act
            Action act = () => new RegistrationFeesService(null!, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpRegistrationFeesService");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_LoggerIsNull_ShouldThrowArgumentNullException(
            IHttpRegistrationFeesService httpRegistrationFeesService)
        {
            // Act
            Action act = () => new RegistrationFeesService(httpRegistrationFeesService, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_RequestIsNull_ShouldThrowArgumentNullException(
            RegistrationFeesService service)
        {
            // Act
            Func<Task> act = () => service.CalculateProducerFeesAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"{ExceptionMessages.ErrorCalculatingProducerFees} (Parameter 'request')");
        }

        [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_RequestIsValid_ShouldReturnResponse(
            RegistrationFeesResponseDto expectedResponse,
            ProducerRegistrationFeesRequestDto request)
        {
            // Arrange
            _httpRegistrationFeesServiceMock.Setup(s => s.CalculateProducerFeesAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.CalculateProducerFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

            [TestMethod, AutoMoqData]
        public async Task CalculateProducerFeesAsync_HttpServiceThrowsException_ShouldLogAndThrowServiceException(
            ProducerRegistrationFeesRequestDto request)
        {
            // Arrange
            var exceptionMessage = "Unexpected error occurred";
            var exception = new Exception(exceptionMessage);

            _httpRegistrationFeesServiceMock.Setup(s => s.CalculateProducerFeesAsync(request, It.IsAny<CancellationToken>()))
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
        public async Task GetResubmissionFeeAsync_ReturnsAResult_ShouldReturnAmount(
            [Frozen] RegulatorDto request,
            [Frozen] decimal expectedAmount
            )
        {
            //Arrange
            _httpRegistrationFeesServiceMock.Setup(i => i.GetResubmissionFeeAsync(request, CancellationToken.None)).ReturnsAsync(expectedAmount);

            //Act
            var result = await _service!.GetResubmissionFeeAsync(request, CancellationToken.None);

            //Assert
            result.Should().Be(expectedAmount);
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ReturnsAResult_ShouldReturnNull(
            [Frozen] RegulatorDto request
            )
        {
            //Arrange
            _httpRegistrationFeesServiceMock.Setup(i => i.GetResubmissionFeeAsync(request, CancellationToken.None)).ReturnsAsync((decimal?)null);

            //Act
            var result = await _service!.GetResubmissionFeeAsync(request, CancellationToken.None);

            //Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetResubmissionFeeAsync_RequestIsNull_ThrowsArgumentException()
        {
            // Act & Assert
            await _service.Invoking(async s => await s!.GetResubmissionFeeAsync(null!, new CancellationToken()))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Error occurred while getting resubmission fee. (Parameter 'request')");
        }
    }
}