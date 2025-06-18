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

        [TestMethod]
        public void Validate_EmptyRegulator_ShouldHaveError()
        {
            // Arrange
            var request = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = RequestorTypes.Reprocessors,
                Regulator = string.Empty,
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = DateTime.UtcNow.AddSeconds(-5)
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Regulator)
                  .WithErrorMessage(ValidationMessages.RegulatorRequired);
        }

        [TestMethod]
        public void Validate_Incorrect_Regulator_ShouldHaveError()
        {
            // Arrange
            var request = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = RequestorTypes.Reprocessors,
                Regulator = "England",
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Regulator)
                  .WithErrorMessage(ValidationMessages.RegulatorTypeInvalid);
        }

        [TestMethod]
        public void Validate_EmptyRequestorTypes_ShouldHaveError()
        {

            // Arrange
            var request = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = null,
                Regulator = "GB-ENG",
                MaterialType = MaterialTypes.Plastic,
                SubmissionDate = DateTime.UtcNow.AddSeconds(-5)
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RequestorType)
                  .WithErrorMessage(ValidationMessages.EmptyRequestorType);
        }

        [TestMethod]
        public void Validate_EmptyMaterialType_ShouldHaveError()
        {
            // Arrange
            var request = new ReprocessorOrExporterRegistrationFeesRequestDto
            {
                RequestorType = RequestorTypes.Reprocessors,
                Regulator = "GB-ENG",
                MaterialType = null,
                SubmissionDate = DateTime.UtcNow.AddSeconds(-5)
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MaterialType)
                  .WithErrorMessage(ValidationMessages.EmptyMaterialType);
        }
    }
}