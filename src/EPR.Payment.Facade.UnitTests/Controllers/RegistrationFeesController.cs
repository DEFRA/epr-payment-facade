using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Controllers;
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
    public class RegistrationFeesControllerTests
    {
        private RegistrationFeesController _controller;
        private Mock<IFeesService> _feesServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _feesServiceMock = new Mock<IFeesService>();
            _controller = new RegistrationFeesController(_feesServiceMock.Object);
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.CalculateProducerFeesAsync(new ProducerRegistrationRequestDto());

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldReturnOk_WhenValidRequest()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                IsLargeProducer = true,
                NumberOfSubsidiaries = 5,
                PayBaseFeeAlone = false
            };
            var expectedResponse = new RegistrationFeeResponseDto { TotalFee = 1000 };
            _feesServiceMock.Setup(service => service.CalculateProducerFeesAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CalculateProducerFeesAsync(request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(expectedResponse);
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                IsLargeProducer = true,
                NumberOfSubsidiaries = 101,
                PayBaseFeeAlone = false
            };
            _feesServiceMock.Setup(service => service.CalculateProducerFeesAsync(request))
                .ThrowsAsync(new ArgumentException("Invalid number of subsidiaries"));

            // Act
            var result = await _controller.CalculateProducerFeesAsync(request);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid number of subsidiaries");
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.CalculateComplianceSchemeFeesAsync(new ComplianceSchemeRegistrationRequestDto());

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldReturnOk_WhenValidRequest()
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
            var result = await _controller.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(expectedResponse);
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                NumberOfLargeProducers = 10,
                NumberOfSmallProducers = 5,
                NumberOfOnlineMarketplaces = 3,
                NumberOfSubsidiaries = 101,
                PayBaseFeeAlone = false
            };
            _feesServiceMock.Setup(service => service.CalculateComplianceSchemeFeesAsync(request))
                .ThrowsAsync(new ArgumentException("Invalid number of subsidiaries"));

            // Act
            var result = await _controller.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid number of subsidiaries");
        }
    }
}
