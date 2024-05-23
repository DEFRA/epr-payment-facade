using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.UnitTests.Services
{
    [TestClass]
    public class FeesServiceTests
    {
        private readonly Mock<IHttpFeesService> _httpFeeServiceMock = new Mock<IHttpFeesService>();

        [TestMethod]
        public async Task GetFee_ReturnsCorrectResponse()
        {
            // Arrange
            var service = new FeesService(_httpFeeServiceMock.Object);
            var expectedResponse = new GetFeesResponseDto { Large = true, Regulator = "regulator", Amount = 199, EffectiveFrom = DateTime.Now.AddDays(-1), EffectiveTo = DateTime.Now.AddDays(10) };
            _httpFeeServiceMock.Setup(s => s.GetFeeAsync(It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(expectedResponse);

            // Act
            var response = await service.GetFeeAsync(true, "regulator");

            // Assert
            response.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task GetFee_NullOrEmptyRegulator_ThrowsArgumentException()
        {
            // Arrange
            var service = new FeesService(_httpFeeServiceMock.Object);

            // Act & Assert
            await service.Invoking(s => s.GetFeeAsync(true, null)).Should().ThrowAsync<ArgumentException>();
            await service.Invoking(s => s.GetFeeAsync(true, "")).Should().ThrowAsync<ArgumentException>();
        }
    }
}
