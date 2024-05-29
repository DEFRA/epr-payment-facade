using EPR.Payment.Facade.Common.Dtos.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPR.Payment.Facade.Services.Fees.Registration.Interfaces;
using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using EPR.Payment.Facade.Controllers.Fees;

namespace EPR.Payment.Facade.UnitTests.Controllers.Registration
{
    [TestClass]
    public class ComplianceSchemeControllerTests
    {
        private Mock<IFeeCalculatorFactory> _feeCalculatorFactoryMock;
        private Mock<ILogger<ComplianceSchemeController>> _loggerMock;

        [TestInitialize]
        public void Setup()
        {
            _feeCalculatorFactoryMock = new Mock<IFeeCalculatorFactory>();
            _loggerMock = new Mock<ILogger<ComplianceSchemeController>>();
        }

        [TestMethod]
        public async Task CalculateFee_ValidRequest_ReturnsOk()
        {
            // Arrange
            var controller = new ComplianceSchemeController(_feeCalculatorFactoryMock.Object, _loggerMock.Object);
            var request = new RegistrationFeeRequest { Subsidiaries = 5, IsLate = false, IsResubmission = false, IsOnlineMarketplace = false };
            var expectedResult = new FeeResponse { Fee = 100 };

            var feeCalculatorServiceMock = new Mock<IFeeCalculatorService<RegistrationFeeRequest>>();
            feeCalculatorServiceMock.Setup(s => s.CalculateFeeAsync(request)).Returns(Task.FromResult(expectedResult));

            _feeCalculatorFactoryMock.Setup(f => f.GetFeeCalculator<RegistrationFeeRequest>()).Returns(feeCalculatorServiceMock.Object);

            // Act
            var result = await controller.CalculateFee(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().Be(expectedResult);
        }

        [TestMethod]
        public async Task CalculateFee_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var controller = new ComplianceSchemeController(_feeCalculatorFactoryMock.Object, _loggerMock.Object);
            var request = new RegistrationFeeRequest { /* Missing required properties */ };

            controller.ModelState.AddModelError("Subsidiaries", "The Subsidiaries field is required.");

            // Act
            var result = await controller.CalculateFee(request);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
        }

        [TestMethod]
        public async Task CalculateFee_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var controller = new ComplianceSchemeController(_feeCalculatorFactoryMock.Object, _loggerMock.Object);
            var request = new RegistrationFeeRequest { Subsidiaries = 5, IsLate = false, IsResubmission = false, IsOnlineMarketplace = false };

            var feeCalculatorServiceMock = new Mock<IFeeCalculatorService<RegistrationFeeRequest>>();
            feeCalculatorServiceMock.Setup(s => s.CalculateFeeAsync(request)).ThrowsAsync(new Exception("Test Exception"));

            _feeCalculatorFactoryMock.Setup(f => f.GetFeeCalculator<RegistrationFeeRequest>()).Returns(feeCalculatorServiceMock.Object);

            // Act
            var result = await controller.CalculateFee(request);

            // Assert
            var objectResult = result.Result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be(500);
        }
    }
}
