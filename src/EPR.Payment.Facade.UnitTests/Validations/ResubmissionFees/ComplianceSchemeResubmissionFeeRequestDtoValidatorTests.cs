using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Validations.ResubmissionFees.ComplianceScheme;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.ResubmissionFees
{
    [TestClass]
    public class ComplianceSchemeResubmissionFeeRequestDtoValidatorTests
    {
        private ComplianceSchemeResubmissionFeeRequestDtoValidator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new ComplianceSchemeResubmissionFeeRequestDtoValidator();
        }

        [TestMethod]
        public void Validate_EmptyRegulator_ShouldHaveError()
        {
            // Arrange
            var request = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = string.Empty,
                ResubmissionDate = DateTime.UtcNow,
                ReferenceNumber = "REF1234",
                MemberCount = 1
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
            var request = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = "INVALID",
                ResubmissionDate = DateTime.UtcNow,
                ReferenceNumber = "REF1234",
                MemberCount = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Regulator)
                .WithErrorMessage(ValidationMessages.RegulatorInvalid);
        }

        [TestMethod]
        public void Validate_EmptyResubmissionDate_ShouldHaveError()
        {
            // Arrange
            var request = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ReferenceNumber = "REF1234",
                MemberCount = 1
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
            var request = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow.AddDays(1),
                ReferenceNumber = "REF1234",
                MemberCount = 1
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
            var request = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.Now, // Not in UTC
                ReferenceNumber = "REF1234",
                MemberCount = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ResubmissionDate)
                .WithErrorMessage(ValidationMessages.ResubmissionDateMustBeUtc);
        }


        [TestMethod]
        public void Validate_ResubmissionDateDefault_ShouldHaveError()
        {
            // Arrange
            var request = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = default(DateTime), // Default value
                ReferenceNumber = "REF1234",
                MemberCount = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ResubmissionDate)
                .WithErrorMessage(ValidationMessages.ResubmissionDateRequired);
        }

        [TestMethod]
        public void Validate_EmptyReferenceNumber_ShouldHaveError()
        {
            // Arrange
            var request = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow,
                ReferenceNumber = string.Empty,
                MemberCount = 1
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ReferenceNumber)
                .WithErrorMessage(ValidationMessages.ReferenceNumberRequired);
        }

        [TestMethod]
        public void Validate_MemberCountLessThanOrEqualToZero_ShouldHaveError()
        {
            // Arrange
            var request = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow,
                ReferenceNumber = "REF1234",
                MemberCount = 0
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MemberCount)
                .WithErrorMessage(ValidationMessages.MemberCountGreaterThanZero);
        }

        [TestMethod]
        public void Validate_ValidRequest_ShouldNotHaveAnyErrors()
        {
            // Arrange
            var request = new ComplianceSchemeResubmissionFeeRequestDto
            {
                Regulator = "GB-ENG",
                ResubmissionDate = DateTime.UtcNow.AddSeconds(-1), // Set to slightly in the past
                ReferenceNumber = "REF1234",
                MemberCount = 5
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

    }
}