using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.Producer;
using EPR.Payment.Facade.Validations.ResubmissionFees.Producer;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.ResubmissionFees
{
    [TestClass]
    public class ProducerResubmissionFeeRequestDtoValidatorTests
    {
        private ProducerResubmissionFeeRequestDtoValidator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new ProducerResubmissionFeeRequestDtoValidator();
        }

        [TestMethod]
        public void Validate_EmptyRegulator_ShouldHaveError()
        {
            // Arrange
            var request = new ProducerResubmissionFeeRequestDto
            {
                Regulator = string.Empty,
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.UtcNow
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
            var request = new ProducerResubmissionFeeRequestDto
            {
                Regulator = "INVALID",
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.UtcNow
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
            var request = new ProducerResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = string.Empty,
                ResubmissionDate = DateTime.UtcNow
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
            var request = new ProducerResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = "REF1234",
                ResubmissionDate = default(DateTime)
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
            var request = new ProducerResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.UtcNow.AddDays(1)
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
            var request = new ProducerResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.Now // Not in UTC
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
            var request = new ProducerResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = "REF1234",
                ResubmissionDate = DateTime.UtcNow.AddSeconds(-1) // Slightly in the past
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}