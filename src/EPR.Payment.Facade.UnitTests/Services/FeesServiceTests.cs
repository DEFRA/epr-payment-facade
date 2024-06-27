using EPR.Payment.Facade.Common.Dtos;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Services;
using EPR.Payment.Facade.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Tests.Services
{
    [TestClass]
    public class FeesServiceTests
    {
        private IFeesService _feesService;
        private Mock<IHttpFeesService> _httpFeesServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _httpFeesServiceMock = new Mock<IHttpFeesService>();
            _feesService = new FeesService(_httpFeesServiceMock.Object);
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldThrowArgumentException_WhenSubsidiariesExceedLimit()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                IsLargeProducer = true,
                NumberOfSubsidiaries = 101,
                PayBaseFeeAlone = false
            };

            // Act
            Func<Task> act = async () => await _feesService.CalculateProducerFeesAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("The number of subsidiaries cannot exceed 100.");
        }

        [TestMethod]
        public async Task CalculateProducerFeesAsync_ShouldCallHttpService_WhenValidRequest()
        {
            // Arrange
            var request = new ProducerRegistrationRequestDto
            {
                IsLargeProducer = true,
                NumberOfSubsidiaries = 5,
                PayBaseFeeAlone = false
            };
            var expectedResponse = new RegistrationFeeResponseDto { TotalFee = 1000 };
            _httpFeesServiceMock.Setup(service => service.CalculateProducerFeesAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var response = await _feesService.CalculateProducerFeesAsync(request);

            // Assert
            response.TotalFee.Should().Be(expectedResponse.TotalFee);
            _httpFeesServiceMock.Verify(service => service.CalculateProducerFeesAsync(request), Times.Once);
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldThrowArgumentException_WhenSubsidiariesExceedLimit()
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

            // Act
            Func<Task> act = async () => await _feesService.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("The number of subsidiaries cannot exceed 100.");
        }

        [TestMethod]
        public async Task CalculateComplianceSchemeFeesAsync_ShouldCallHttpService_WhenValidRequest()
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
            _httpFeesServiceMock.Setup(service => service.CalculateComplianceSchemeFeesAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var response = await _feesService.CalculateComplianceSchemeFeesAsync(request);

            // Assert
            response.TotalFee.Should().Be(expectedResponse.TotalFee);
            _httpFeesServiceMock.Verify(service => service.CalculateComplianceSchemeFeesAsync(request), Times.Once);
        }
    }
}
