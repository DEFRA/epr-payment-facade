﻿using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services;
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
            _httpFeeServiceMock.Setup(s => s.GetFee(It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(expectedResponse);

            // Act
            var response = await service.GetFee(true, "regulator");

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public async Task GetFee_NullOrEmptyRegulator_ThrowsArgumentException()
        {
            // Arrange
            var service = new FeesService(_httpFeeServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.GetFee(true, null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.GetFee(true, ""));
        }
    }
}
