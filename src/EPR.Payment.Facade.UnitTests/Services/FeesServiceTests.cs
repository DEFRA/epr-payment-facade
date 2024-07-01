using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services;
using EPR.Payment.Facade.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Tests.Services
{
    [TestClass]
    public class FeesServiceTests
    {
        private Mock<IHttpFeesService> _mockHttpFeesService;
        private Mock<ILogger<FeesService>> _mockLogger;
        private FeesService _feesService;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpFeesService = new Mock<IHttpFeesService>();
            _mockLogger = new Mock<ILogger<FeesService>>();
            _feesService = new FeesService(_mockHttpFeesService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldReturnFee_WhenRequestIsValid()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                PayBaseFee = true
            };

            var expectedResponse = new RegistrationFeeResponseDto { TotalFee = 1000 };
            _mockHttpFeesService.Setup(s => s.CalculateProducerFeesAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _feesService.CalculateProducerFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldThrowArgumentException_WhenNumberOfSubsidiariesExceeds100()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 101,
                PayBaseFee = true
            };

            // Act
            Func<Task> act = async () => await _feesService.CalculateProducerFeesAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Number of subsidiaries cannot exceed 100.");
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldLogErrorAndThrowException_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                PayBaseFee = true
            };

            var exceptionMessage = "Error calculating producer fees";
            _mockHttpFeesService.Setup(s => s.CalculateProducerFeesAsync(request))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            Func<Task> act = async () => await _feesService.CalculateProducerFeesAsync(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage(exceptionMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error calculating producer fees")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldReturnFee_WhenRequestIsValid()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                Producers = new System.Collections.Generic.List<ProducerSubsidiaryInfo>
                {
                    new ProducerSubsidiaryInfo { ProducerType = "L", NumberOfSubsidiaries = 10, PayBaseFee = true },
                    new ProducerSubsidiaryInfo { ProducerType = "S", NumberOfSubsidiaries = 5, PayBaseFee = true }
                },
                PayComplianceSchemeBaseFee = true
            };

            var expectedResponse = new RegistrationFeeResponseDto { TotalFee = 3000 };
            _mockHttpFeesService.Setup(s => s.CalculateComplianceSchemeFeesAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _feesService.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldThrowArgumentException_WhenNumberOfSubsidiariesExceeds100()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                Producers = new System.Collections.Generic.List<ProducerSubsidiaryInfo>
                {
                    new ProducerSubsidiaryInfo { ProducerType = "L", NumberOfSubsidiaries = 101, PayBaseFee = true }
                },
                PayComplianceSchemeBaseFee = true
            };

            // Act
            Func<Task> act = async () => await _feesService.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Number of subsidiaries per producer must be between 0 and 100.");
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldThrowArgumentException_WhenAnyProducerHasInvalidNumberOfSubsidiaries()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                Producers = new System.Collections.Generic.List<ProducerSubsidiaryInfo>
                {
                    new ProducerSubsidiaryInfo { ProducerType = "L", NumberOfSubsidiaries = 50, PayBaseFee = true },
                    new ProducerSubsidiaryInfo { ProducerType = "S", NumberOfSubsidiaries = 101, PayBaseFee = true } // Invalid
                },
                PayComplianceSchemeBaseFee = true
            };

            // Act
            Func<Task> act = async () => await _feesService.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Number of subsidiaries per producer must be between 0 and 100.");
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldLogErrorAndThrowException_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                Producers = new System.Collections.Generic.List<ProducerSubsidiaryInfo>
                {
                    new ProducerSubsidiaryInfo { ProducerType = "L", NumberOfSubsidiaries = 10, PayBaseFee = true },
                    new ProducerSubsidiaryInfo { ProducerType = "S", NumberOfSubsidiaries = 5, PayBaseFee = true }
                },
                PayComplianceSchemeBaseFee = true
            };

            var exceptionMessage = "Error calculating compliance scheme fees";
            _mockHttpFeesService.Setup(s => s.CalculateComplianceSchemeFeesAsync(request))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            Func<Task> act = async () => await _feesService.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage(exceptionMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error calculating compliance scheme fees")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
