using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Tests.Services
{
    [TestClass]
    public class FeesServiceTests
    {
        private Mock<IHttpFeesService> _mockHttpFeesService;
        private FeesService _feesService;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpFeesService = new Mock<IHttpFeesService>();
            _feesService = new FeesService(_mockHttpFeesService.Object);
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
        public async Task CalculateComplianceSchemeFeesAsync_ShouldReturnFee_WhenRequestIsValid()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                Producers = new List<ProducerSubsidiaryInfo>
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
                Producers = new List<ProducerSubsidiaryInfo>
                {
                    new ProducerSubsidiaryInfo { ProducerType = "L", NumberOfSubsidiaries = 101, PayBaseFee = true }
                },
                PayComplianceSchemeBaseFee = true
            };

            // Act
            Func<Task> act = async () => await _feesService.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Number of subsidiaries cannot exceed 100.");
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldThrowArgumentException_WhenAnyProducerHasInvalidNumberOfSubsidiaries()
        {
            // Arrange
            var request = new ComplianceSchemeRegistrationRequestDto
            {
                Producers = new List<ProducerSubsidiaryInfo>
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
                .WithMessage("Number of subsidiaries cannot exceed 100.");
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldReturnCorrectFees_ForLargeProducerWith25Subsidiaries()
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

            _mockHttpFeesService.Setup(s => s.CalculateProducerFeesAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _feesService.CalculateProducerFeesAsync(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
