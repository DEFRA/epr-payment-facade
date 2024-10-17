using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using EPR.Payment.Facade.Services.ResubmissionFees.Producer;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Facade.UnitTests.Services.RegistrationFees.Producer
{
    [TestClass]
    public class ProducerResubmissionFeesServiceTests_
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

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ReturnsAResult_ShouldReturnAmount(
            [Frozen] RegulatorDto request,
            [Frozen] decimal expectedAmount)
        {
            //Arrange
            _httpProducerResubmissionFeesService.Setup(i => i.GetResubmissionFeeAsync(request, CancellationToken.None)).ReturnsAsync(expectedAmount);

            //Act
            var result = await _service!.GetResubmissionFeeAsync(request, CancellationToken.None);

            //Assert
            result.Should().Be(expectedAmount);
        }

        [TestMethod, AutoMoqData]
        public async Task GetResubmissionFeeAsync_ReturnsAResult_ShouldReturnNull(
            [Frozen] RegulatorDto request)
        {
            //Arrange
            _httpProducerResubmissionFeesService.Setup(i => i.GetResubmissionFeeAsync(request, CancellationToken.None)).ReturnsAsync((decimal?)null);

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