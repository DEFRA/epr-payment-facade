using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Controllers.RegistrationFees;
using EPR.Payment.Facade.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Tests.Controllers
{
    [TestClass]
    public class ProducersControllerTests
    {
        private Mock<IFeesService> _mockFeesService;
        private ProducersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockFeesService = new Mock<IFeesService>();
            _controller = new ProducersController(_mockFeesService.Object);
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                PayBaseFee = true
            };

            var expectedResponse = new RegistrationFeeResponseDto { TotalFee = 1000 };
            _mockFeesService.Setup(s => s.CalculateProducerFeesAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnBadRequest_WhenNumberOfSubsidiariesIsNegative()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = -1,
                PayBaseFee = true
            };

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Number of subsidiaries must be between 0 and 100.");
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnBadRequest_WhenNumberOfSubsidiariesExceeds100()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 101,
                PayBaseFee = true
            };

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Number of subsidiaries must be between 0 and 100.");
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnBadRequest_WhenProducerTypeIsInvalid()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "X",
                NumberOfSubsidiaries = 10,
                PayBaseFee = true
            };

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("ProducerType must be 'L' for Large or 'S' for Small.");
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid model state");

            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                PayBaseFee = true
            };

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeOfType<SerializableError>();
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                PayBaseFee = true
            };

            _mockFeesService.Setup(s => s.CalculateProducerFeesAsync(request))
                .ThrowsAsync(new ArgumentException("Invalid number of subsidiaries"));

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Invalid number of subsidiaries");
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                PayBaseFee = true
            };

            _mockFeesService.Setup(s => s.CalculateProducerFeesAsync(request))
                .ThrowsAsync(new Exception("Internal server error"));

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Internal server error");
        }

        [TestMethod]
        public async Task CalculateFeesAsync_ShouldReturnOk_WithCorrectFees_ForLargeProducerWith25Subsidiaries()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 25
            };

            var expectedResponse = new RegistrationFeeResponseDto
            {
                BaseFee = 1658.00m,
                SubsidiariesFee = 12040.00m,
                ProducersFee = 0.00m,
                TotalFee = 13698.00m,
                FeeBreakdowns = new List<FeeBreakdown>
                {
                    new FeeBreakdown { Description = "Base Fee for Large Producer", Amount = 1658.00m },
                    new FeeBreakdown { Description = "Fee for first 20 subsidiaries", Amount = 11160.00m },
                    new FeeBreakdown { Description = "Fee for additional 5 subsidiaries", Amount = 700.00m }
                }
            };

            _mockFeesService.Setup(s => s.CalculateProducerFeesAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CalculateFeesAsync(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
