﻿using EPR.Payment.Facade.Common.Constants;
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
        public void Validate_ShouldHaveError_WhenProducerTypeIsInvalid()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "X",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsOnlineMarketplace = false
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
        public void Validate_ShouldNotHaveError_ForUpperCaseProducerTypeL()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsOnlineMarketplace = false
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
            }
        }

        [TestMethod]
        public void Validate_ShouldNotHaveError_ForLowerCaseProducerTypeL()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "l",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsOnlineMarketplace = false
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
            }
        }

        [TestMethod]
        public void Validate_ShouldNotHaveError_ForUpperCaseProducerTypeS()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "S",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsOnlineMarketplace = false
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
            }
        }

        [TestMethod]
        public void Validate_ShouldNotHaveError_ForLowerCaseProducerTypeS()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "s",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsOnlineMarketplace = false
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
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
                Regulator = RegulatorConstants.GBENG,
                IsOnlineMarketplace = false
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
                IsOnlineMarketplace = false
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
        public void Validate_ShouldHaveError_WhenProducerTypeIsEmpty_AndNumberOfSubsidiariesIsZero()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = string.Empty, // No base fee required
                NumberOfSubsidiaries = 0,
                Regulator = RegulatorConstants.GBENG,
                IsOnlineMarketplace = false
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldHaveValidationErrorFor(x => x.NumberOfSubsidiaries)
                    .WithErrorMessage("Number of subsidiaries must be greater than 0 when ProducerType is empty.");
            }
        }

        [TestMethod]
        public void Validate_ShouldNotHaveError_WhenRequestIsValid_WithProducerType()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = "L",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsOnlineMarketplace = false
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

        [TestMethod]
        public void Validate_ShouldNotHaveError_WhenProducerTypeIsEmpty_AndNumberOfSubsidiariesIsGreaterThanZero()
        {
            // Arrange
            var request = new ProducerRegistrationFeesRequestDto
            {
                ProducerType = string.Empty, // No base fee required
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsOnlineMarketplace = false
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            using (new AssertionScope())
            {
                result.ShouldNotHaveValidationErrorFor(x => x.NumberOfSubsidiaries);
                result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
            }
        }

        [TestMethod]
        public void Validate_ShouldNotHaveError_WhenRegulatorIsValid()
        {
            // Arrange
            var validRegulators = new[] { RegulatorConstants.GBENG, RegulatorConstants.GBSCT, RegulatorConstants.GBWLS, RegulatorConstants.GBNIR };

            foreach (var regulator in validRegulators)
            {
                var request = new ProducerRegistrationFeesRequestDto
                {
                    ProducerType = "L",
                    NumberOfSubsidiaries = 10,
                    Regulator = regulator,
                    IsOnlineMarketplace = false
                };

                // Act
                var result = _validator.TestValidate(request);

                // Assert
                using (new AssertionScope())
                {
                    result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
                }
            }
        }
    }
}