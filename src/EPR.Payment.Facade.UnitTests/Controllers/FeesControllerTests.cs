using EPR.Payment.Facade.Controllers;
using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Tests
{
    [TestClass]
    public class FeesControllerTests
    {
        private FeesController? _controller;
        private Mock<IFeesService>? _feesServiceMock;
        private Mock<ILogger<FeesController>>? _loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            _feesServiceMock = new Mock<IFeesService>();
            _loggerMock = new Mock<ILogger<FeesController>>();

            _controller = new FeesController(_feesServiceMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task GetFee_ValidRequest_ReturnsOk()
        {
            // Arrange
            var expectedResponse = new GetFeesResponseDto { Large = true, Regulator = "regulator", Amount = 199, EffectiveFrom = DateTime.Now.AddDays(-1), EffectiveTo = DateTime.Now.AddDays(10) };
            _feesServiceMock.Setup(service => service.GetFee(It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetFee(true, "regulator");

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.AreEqual(expectedResponse, okResult.Value);
        }

        [TestMethod]
        public async Task GetFee_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _feesServiceMock.Setup(service => service.GetFee(It.IsAny<bool>(), It.IsAny<string>())).ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.GetFee(true, "regulator");

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult)); 
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode); 
        }        
    }
}
