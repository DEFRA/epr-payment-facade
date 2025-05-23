using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Validations.RegistrationFees.ReprocessorOrExporter;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.RegistrationFees
{
    [TestClass]
    public class ReprocessorOrExporterRegistrationFeesRequestDtoValidatorTests
    {
        private ReprocessorOrExporterRegistrationFeesRequestDtoValidator _validator = null!;
        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new ReprocessorOrExporterRegistrationFeesRequestDtoValidator();
        }

        [TestMethod]
        public void Validator_Should_Fail_When_MaterialType_Is_Empty()
        {
            // Arrange
            var dto = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                            
                Regulator = "GB-ENG",
                SubmissionDate = DateTime.UtcNow,
                MaterialType = null,
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MaterialType)
                  .WithErrorMessage(ValidationMessages.MaterialTypeInvalid);
        }

        [TestMethod]
        public void Should_Have_Error_When_SubmissionDate_Is_Empty()
        {
            var model = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = RequestorTypes.Reprocessors,
                Regulator = "GB-ENG",
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = default
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.SubmissionDate)
            .WithErrorMessage(ValidationMessages.ReprocessorExporterDateRequired);
        }

        [TestMethod]
        public void Should_Have_Error_When_SubmissionDate_Is_Not_Utc()
        {
            var model = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = RequestorTypes.Reprocessors,
                Regulator = "GB-ENG",
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local)
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.SubmissionDate)
            .WithErrorMessage(ValidationMessages.ResubmissionDateMustBeUtc);
        }

        [TestMethod]
        public void Should_Have_Error_When_SubmissionDate_Is_In_The_Future()
        {
            var model = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = RequestorTypes.Reprocessors,
                Regulator = "GB-ENG",
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = DateTime.UtcNow.AddMinutes(5)
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.SubmissionDate)
            .WithErrorMessage(ValidationMessages.RexExFutureResubmissionDate);
        }
    }
}