using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.Producer;
using EPR.Payment.Facade.Validations.RegistrationFees.Producer;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.RegistrationFees
{
    [TestClass]
    public class ProducerRegistrationFeeRequestDtoV2ValidatorTests
    {
        private ProducerFeesRequestDtoV2Validator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new ProducerFeesRequestDtoV2Validator();
        }

        [TestMethod]
        public void Validate_InvalidProducerType_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "X",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
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
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
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
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "large",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
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
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "SMALL",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
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
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "small",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
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
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = -5,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
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
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = string.Empty,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Regulator)
                  .WithErrorMessage(ValidationMessages.RegulatorRequired);
        }

        [TestMethod]
        public void Validate_EmptyProducerType_ShouldHaveError()
        {
            // Arrange
            var validProducerTypes = new List<string> { "LARGE", "SMALL" };
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = string.Empty,
                NumberOfSubsidiaries = 0,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ProducerType)
                  .WithErrorMessage(ValidationMessages.ProducerTypeInvalid + string.Join(", ", validProducerTypes));
        }

        [TestMethod]
        public void Validate_NumberOfOMPSubsidiaries_ShouldBeLessThanOrEqualToNumberOfSubsidiaries()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "Large",
                NumberOfSubsidiaries = 10,
                NoOfSubsidiariesOnlineMarketplace = 11,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NoOfSubsidiariesOnlineMarketplace)
                  .WithErrorMessage(ValidationMessages.NumberOfOMPSubsidiariesLessThanOrEqualToNumberOfSubsidiaries);
        }

        [TestMethod]
        public void Validate_ValidRequestWithProducerType_ShouldNotHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
            result.ShouldNotHaveValidationErrorFor(x => x.NumberOfSubsidiaries);
            result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Validate_ProducerTypeAndGreaterThanZeroSubsidiaries_ShouldNotHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
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
                var request = new ProducerFeesRequestV2Dto
                {
                    ProducerType = "LARGE",
                    NumberOfSubsidiaries = 10,
                    Regulator = regulator,
                    ApplicationReferenceNumber = "A123",
                    SubmissionDate = DateTime.UtcNow,
                    FileId = Guid.NewGuid(),
                    ExternalId = Guid.NewGuid(),
                    InvoicePeriod = new DateTimeOffset(),
                    PayerId = 1,
                    PayerTypeId = 1
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
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 0,
                Regulator = RegulatorConstants.GBENG,
                NoOfSubsidiariesOnlineMarketplace = -5,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
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
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = string.Empty,
                ApplicationReferenceNumber = string.Empty,
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ApplicationReferenceNumber)
                  .WithErrorMessage(ValidationMessages.ApplicationReferenceNumberRequired);
        }

        [TestMethod]
        public void Validate_InvalidSubmissionDate_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = default,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SubmissionDate)
                  .WithErrorMessage(ValidationMessages.InvalidSubmissionDate);
        }

        [TestMethod]
        public void Validate_FutureSubmissionDate_ShouldNotHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow.AddDays(10),
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SubmissionDate);
        }

        [TestMethod]
        public void Validate_SubmissionDateNotUtc_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "LARGE",
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.Now,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SubmissionDate)
                .WithErrorMessage(ValidationMessages.SubmissionDateMustBeUtc);
        }

        [TestMethod]
        public void Validator_Should_Fail_When_FileId_Is_Empty()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "large",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.Empty,
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FileId)
                  .WithErrorMessage(ValidationMessages.FileIdRequired);
        }

        [TestMethod]
        public void Validator_Should_Fail_When_ExternalId_Is_Empty()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "large",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.Empty,
                ExternalId = Guid.Empty,
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ExternalId)
                  .WithErrorMessage(ValidationMessages.ExternalIdRequired);
        }

        public void Validator_Should_Fail_When_InvoicePeriod_Is_Not_Valid()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "large",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoicePeriod)
                  .WithErrorMessage(ValidationMessages.InvoicePeriodRequired);
        }

        [TestMethod]
        public void Validator_Should_Fail_When_PayerTypeId_Is_Not_Valid()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "large",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 1,
                PayerTypeId = 0
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PayerTypeId)
                  .WithErrorMessage(ValidationMessages.PayerTypeIdRequired);
        }

        [TestMethod]
        public void Validator_Should_Fail_When_PayerId_Is_Not_Valid()
        {
            // Arrange
            var request = new ProducerFeesRequestV2Dto
            {
                ProducerType = "large",  // Lowercase
                NumberOfSubsidiaries = 10,
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "A123",
                SubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = new DateTimeOffset(),
                PayerId = 0,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PayerId)
                  .WithErrorMessage(ValidationMessages.PayerIdRequired);
        }
    }
}