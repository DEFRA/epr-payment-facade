using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Dtos.Response.RegistrationFees.ReProcessorOrExporter;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Validations.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Validations.RegistrationFees.ReprocessorOrExporter;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                RequestorType = "Reprocessor",
                Regulator = "GB-ENG",
                SubmissionDate = DateTime.UtcNow,
                MaterialType = string.Empty,
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
                RequestorType = "Reprocessor",
                Regulator = "GB-ENG",
                MaterialType = "Paper",
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
                RequestorType = "Reprocessor",
                Regulator = "GB-ENG",
                MaterialType = "Paper",
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
                RequestorType = "Reprocessor",
                Regulator = "GB-ENG",
                MaterialType = "Paper",
                SubmissionDate = DateTime.UtcNow.AddMinutes(5)
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.SubmissionDate)
            .WithErrorMessage(ValidationMessages.RexExFutureResubmissionDate);
        }
    }
}