using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Controllers;
using EPR.Payment.Facade.Controllers.RegistrationFees;
using EPR.Payment.Facade.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Tests.Controllers
{
    [TestClass]
    public class ComplianceSchemeControllerTests
    {
        private ComplianceSchemeController _controller;
        private Mock<IFeesService> _feesServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _feesServiceMock = new Mock<IFeesService>();
            _controller = new ComplianceSchemeController(_feesServiceMock.Object);
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.CalculateFeesAsync(new ComplianceSchemeRegistrationRequestDto());

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnOk_WhenValidRequest()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                NumberOfLargeProducers = 10,
                NumberOfSmallProducers = 5,
                NumberOfOnlineMarketplaces = 3,
                NumberOfSubsidiaries = 20,
                PayBaseFeeAlone = false
            };
            var expectedResponse = new RegistrationFeeResponseDto { TotalFee = 5000 };
            _feesServiceMock.Setup(service => service.CalculateComplianceSchemeFeesAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(expectedResponse);
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                NumberOfLargeProducers = 10,
                NumberOfSmallProducers = 5,
                NumberOfOnlineMarketplaces = 3,
                NumberOfSubsidiaries = 101,  // Invalid number of subsidiaries
                PayBaseFeeAlone = false
            };

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Number of subsidiaries must be between 0 and 100.");
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnBadRequest_WhenGeneralExceptionThrown()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                NumberOfLargeProducers = 10,
                NumberOfSmallProducers = 5,
                NumberOfOnlineMarketplaces = 3,
                NumberOfSubsidiaries = 20,
                PayBaseFeeAlone = false
            };
            _feesServiceMock.Setup(service => service.CalculateComplianceSchemeFeesAsync(request))
                .ThrowsAsync(new Exception("General error"));

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("General error");
        }
    }
}
