using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Validations.RegistrationFees;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.RegistrationFees
{
    [TestClass]
    public class ProducerRegistrationFeeRequestDtoValidatorTests
    {
        private ProducerFeesRequestDtoValidator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new ProducerFeesRequestDtoValidator();
        }

        [TestMethod]
        public void Validate_InvalidProducerType_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "X",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ProducerType)
                  .WithErrorMessage(ValidationMessages.ProducerTypeInvalid + "LARGE, SMALL");
        }

        [TestMethod]
        public void Validate_UpperCaseProducerTypeLarge_ShouldNotHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
        }

        [TestMethod]
        public void Validate_LowerCaseProducerTypeLarge_ShouldNotHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "large",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
        }

        [TestMethod]
        public void Validate_UpperCaseProducerTypeSmall_ShouldNotHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "SMALL",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
        }

        [TestMethod]
        public void Validate_LowerCaseProducerTypeSmall_ShouldNotHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "small",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
        }

        [TestMethod]
        public void Validate_NumberOfSubsidiariesLessThanZero_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = -5,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NumberOfSubsidiaries)
                  .WithErrorMessage(ValidationMessages.NumberOfSubsidiariesRange);
        }

        [TestMethod]
        public void Validate_EmptyRegulator_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = string.Empty,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Regulator)
                  .WithErrorMessage(ValidationMessages.RegulatorRequired);
        }

        [TestMethod]
        public void Validate_EmptyProducerTypeAndZeroSubsidiaries_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = string.Empty, // No base fee required
                NumberOfSubsidiaries = 0,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NumberOfSubsidiaries)
                  .WithErrorMessage(ValidationMessages.NumberOfSubsidiariesRequiredWhenProducerTypeEmpty);
        }

        [TestMethod]
        public void Validate_ValidRequestWithProducerType_ShouldNotHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
            result.ShouldNotHaveValidationErrorFor(x => x.NumberOfSubsidiaries);
            result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Validate_EmptyProducerTypeAndGreaterThanZeroSubsidiaries_ShouldNotHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = string.Empty, // No base fee required
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.NumberOfSubsidiaries);
            result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Validate_ValidRegulator_ShouldNotHaveError()
        {
            // Arrange
            var validRegulators = new[] { RegulatorConstants.GBENG, RegulatorConstants.GBSCT, RegulatorConstants.GBWLS, RegulatorConstants.GBNIR };

            foreach (var regulator in validRegulators)
            {
                var request = new ProducerFeesRequestDto
                {
                    ProducerType = "LARGE",
                    NumberOfSubsidiaries = 10,
                    Regulator = regulator,
                    IsProducerOnlineMarketplace = false,
                    ApplicationReferenceNumber = "A123"
                };

                // Act
                var result = _validator.TestValidate(request);

                // Assert
                result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
            }
        }

        [TestMethod]
        public void Validate_NoOfSubsidiariesOnlineMarketplaceLessThanZero_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 0,
                Regulator = RegulatorConstants.GBENG,
                IsProducerOnlineMarketplace = false,
                NoOfSubsidiariesOnlineMarketplace = -5,
                ApplicationReferenceNumber = "A123"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NoOfSubsidiariesOnlineMarketplace)
                  .WithErrorMessage(ValidationMessages.NoOfSubsidiariesOnlineMarketplaceRange);
        }

        [TestMethod]
        public void Validate_EmptyApplicationReferenceNumber_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestDto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = string.Empty,
                IsProducerOnlineMarketplace = false,
                ApplicationReferenceNumber = string.Empty
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ApplicationReferenceNumber)
                  .WithErrorMessage(ValidationMessages.ApplicationReferenceNumberRequired);
        }

    }
}