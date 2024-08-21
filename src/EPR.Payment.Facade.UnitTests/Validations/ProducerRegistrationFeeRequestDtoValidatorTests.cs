using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees;
using EPR.Payment.Facade.Validations;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations
{
    [TestClass]
    public class ProducerRegistrationFeeRequestDtoValidatorTests
    {
        private ProducerRegistrationFeesRequestDtoValidator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new ProducerRegistrationFeesRequestDtoValidator();
        }

        [TestMethod]
        public void Validate_ShouldHaveError_WhenProducerTypeIsEmpty()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = string.Empty,
                NumberOfSubsidiaries = 10,
                Regulator = "GB-ENG",
                IsOnlineMarketplace = false,
                PayBaseFee = true
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldHaveValidationErrorFor(x => x.ProducerType)
                    .WithErrorMessage(ValidationMessages.ProducerTypeRequired);

                result.ShouldHaveValidationErrorFor(x => x.ProducerType)
                    .WithErrorMessage(ValidationMessages.ProducerTypeInvalid);
            }
        }

        [TestMethod]
        public void Validate_ShouldHaveError_WhenProducerTypeIsInvalid()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "X",
                NumberOfSubsidiaries = 10,
                Regulator = "GB-ENG",
                IsOnlineMarketplace = false,
                PayBaseFee = true
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldHaveValidationErrorFor(x => x.ProducerType)
                    .WithErrorMessage(ValidationMessages.ProducerTypeInvalid);
            }
        }

        [TestMethod]
        public void Validate_ShouldHaveError_WhenNumberOfSubsidiariesIsOutOfRange()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 101,
                Regulator = "GB-ENG",
                IsOnlineMarketplace = false,
                PayBaseFee = true
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldHaveValidationErrorFor(x => x.NumberOfSubsidiaries)
                    .WithErrorMessage(ValidationMessages.NumberOfSubsidiariesRange);
            }
        }

        [TestMethod]
        public void Validate_ShouldHaveError_WhenRegulatorIsEmpty()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                Regulator = string.Empty,
                IsOnlineMarketplace = false,
                PayBaseFee = true
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldHaveValidationErrorFor(x => x.Regulator)
                    .WithErrorMessage(ValidationMessages.RegulatorRequired);
            }
        }

        [TestMethod]
        public void Validate_ShouldNotHaveError_WhenRequestIsValid()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                Regulator = "GB-ENG",
                IsOnlineMarketplace = false,
                PayBaseFee = true
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
                result.ShouldNotHaveValidationErrorFor(x => x.NumberOfSubsidiaries);
                result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
            }
        }
    }
}
