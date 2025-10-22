using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Validations.ResubmissionFees.Producer;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.ResubmissionFees
{
    [TestClass]
    public class ProducerResubmissionFeeRequestV2DtoValidatorTests
    {
        private ProducerResubmissionFeeRequestV2DtoValidator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new ProducerResubmissionFeeRequestV2DtoValidator();
        }

        [TestMethod]
        public void Validate_EmptyRegulator_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = string.Empty,
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = DateTimeOffset.Now,
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
        public void Validate_InvalidRegulator_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "INVALID",
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = DateTimeOffset.Now,
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Regulator)
                .WithErrorMessage(ValidationMessages.RegulatorInvalid);
        }

        [TestMethod]
        public void Validate_EmptyReferenceNumber_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = string.Empty,
                ResubmissionDate = DateTime.UtcNow,
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = DateTimeOffset.Now,
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ReferenceNumber)
                .WithErrorMessage(ValidationMessages.ReferenceNumberRequired);
        }

        [TestMethod]
        public void Validate_EmptyResubmissionDate_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = "REF1234",
                ResubmissionDate = default(DateTime),
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = DateTimeOffset.Now,
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ResubmissionDate)
                .WithErrorMessage(ValidationMessages.ResubmissionDateRequired);
        }

        [TestMethod]
        public void Validate_ResubmissionDateInFuture_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.UtcNow.AddDays(1),
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = DateTimeOffset.Now,
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ResubmissionDate)
                .WithErrorMessage(ValidationMessages.FutureResubmissionDate);
        }

        [TestMethod]
        public void Validate_ResubmissionDateNotUtc_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.Now, // Not in UTC
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = DateTimeOffset.Now,
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ResubmissionDate)
                .WithErrorMessage(ValidationMessages.ResubmissionDateMustBeUtc);
        }

        [TestMethod]
        public void Validate_ValidRequest_ShouldNotHaveAnyErrors()
        {
            // Arrange
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.UtcNow.AddSeconds(-1), // Slightly in the past
                FileId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                InvoicePeriod = DateTimeOffset.Now,
                PayerId = 1,
                PayerTypeId = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validator_Should_Fail_When_FileId_Is_Empty()
        {
            // Arrange
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow.AddSeconds(-1), // Set to slightly in the past
                ReferenceNumber = "REF1234",
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
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow.AddSeconds(-1), // Set to slightly in the past
                ReferenceNumber = "REF1234",
                FileId = Guid.NewGuid(),
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
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow.AddSeconds(-1), // Set to slightly in the past
                ReferenceNumber = "REF1234",
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
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow.AddSeconds(-1), // Set to slightly in the past
                ReferenceNumber = "REF1234",
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
            var request = new ProducerResubmissionFeeRequestV2Dto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow.AddSeconds(-1), // Set to slightly in the past
                ReferenceNumber = "REF1234",
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