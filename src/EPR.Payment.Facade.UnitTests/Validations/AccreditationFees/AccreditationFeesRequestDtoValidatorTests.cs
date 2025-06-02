using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.AccreditationFees;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Validations.AccreditationFees;

namespace EPR.Payment.Facade.UnitTests.Validations.AccreditationFees
{
    [TestClass]
    public class AccreditationFeesRequestDtoValidatorTests
    {
        private ReprocessorOrExporterAccreditationFeesRequestDtoValidator _validator;

        [TestInitialize]
        public void Setup() => _validator = new ReprocessorOrExporterAccreditationFeesRequestDtoValidator();

        private ReprocessorOrExporterAccreditationFeesRequestDto CreateValidDto()
        {
            return new ReprocessorOrExporterAccreditationFeesRequestDto
            {
                Regulator = "GB-ENG",
                SubmissionDate = DateTime.UtcNow.AddSeconds(-1),
                TonnageBand = (TonnageBands)Enum.GetValues(typeof(TonnageBands)).GetValue(0),
                RequestorType = RequestorTypes.Exporters,
                MaterialType = (MaterialTypes)Enum.GetValues(typeof(MaterialTypes)).GetValue(0),
                NumberOfOverseasSites = 1
            };
        }

        [TestMethod]
        public void Validate_FullyValidDto_ShouldHaveNoErrors()
        {
            var dto = CreateValidDto();
            var result = _validator.Validate(dto);
            Assert.IsTrue(result.IsValid, "Expected no validation errors but found: " + string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
        }

        [TestMethod]
        public void Validate_RegulatorEmpty_ShouldHaveRequiredError()
        {
            var dto = CreateValidDto();
            dto.Regulator = string.Empty;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);

            var hasReferenceRequired = result.Errors.Any(e => e.PropertyName == nameof(dto.Regulator) && e.ErrorMessage == ValidationMessages.RegulatorRequired);
            Assert.IsTrue(hasReferenceRequired, $"Expected an error '{ValidationMessages.RegulatorRequired}' for Regulator but got: {string.Join(", ", result.Errors.Where(e => e.PropertyName == nameof(dto.Regulator)).Select(e => e.ErrorMessage))}");
        }

        [TestMethod]
        public void Validate_RegulatorInvalid_ShouldHaveInvalidError()
        {
            var dto = CreateValidDto();
            dto.Regulator = "INVALID";
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.Regulator));
            Assert.IsNotNull(error);
            Assert.AreEqual(ValidationMessages.RegulatorInvalid, error.ErrorMessage);
        }

        [TestMethod]
        public void Validate_SubmissionDateInFuture_ShouldHaveError()
        {
            var dto = CreateValidDto();
            dto.SubmissionDate = DateTime.UtcNow.AddDays(1);
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.SubmissionDate));
            Assert.IsNotNull(error);
        }

        [TestMethod]
        public void Validate_TonnageBandNull_ShouldHaveEmptyError()
        {
            var dto = CreateValidDto();
            dto.TonnageBand = null;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.TonnageBand));
            Assert.IsNotNull(error);
            Assert.AreEqual(ValidationMessages.EmptyTonnageBand, error.ErrorMessage);
        }

        [TestMethod]
        public void Validate_TonnageBandOutOfRange_ShouldHaveInvalidError()
        {
            var dto = CreateValidDto();
            dto.TonnageBand = (TonnageBands)999;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.TonnageBand));
            Assert.IsNotNull(error);
            StringAssert.StartsWith(error.ErrorMessage, ValidationMessages.InvalidTonnageBand);
        }

        [TestMethod]
        public void Validate_RequestorTypeNull_ShouldHaveEmptyError()
        {
            var dto = CreateValidDto();
            dto.RequestorType = null;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.RequestorType));
            Assert.IsNotNull(error);
            Assert.AreEqual(ValidationMessages.EmptyRequestorType, error.ErrorMessage);
        }

        [TestMethod]
        public void Validate_RequestorTypeOutOfRange_ShouldHaveInvalidError()
        {
            var dto = CreateValidDto();
            dto.RequestorType = (RequestorTypes)999;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.RequestorType));
            Assert.IsNotNull(error);
            StringAssert.StartsWith(error.ErrorMessage, ValidationMessages.InvalidRequestorType);
        }

        [TestMethod]
        public void Validate_MaterialTypeNull_ShouldHaveEmptyError()
        {
            var dto = CreateValidDto();
            dto.MaterialType = null;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.MaterialType));
            Assert.IsNotNull(error);
            Assert.AreEqual(ValidationMessages.EmptyMaterialType, error.ErrorMessage);
        }

        [TestMethod]
        public void Validate_MaterialTypeOutOfRange_ShouldHaveInvalidError()
        {
            var dto = CreateValidDto();
            dto.MaterialType = (MaterialTypes)999;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.MaterialType));
            Assert.IsNotNull(error);
            StringAssert.StartsWith(error.ErrorMessage, ValidationMessages.InvalidMaterialType);
        }

        [TestMethod]
        public void Validate_Exporter_ZeroSites_ShouldHaveInvalidSiteError()
        {
            var dto = CreateValidDto();
            dto.RequestorType = RequestorTypes.Exporters;
            dto.NumberOfOverseasSites = 0;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.NumberOfOverseasSites));
            Assert.IsNotNull(error);
            // The GreaterThan rule uses FluentValidation's default message for zero.
            StringAssert.Contains(error.ErrorMessage, "must be greater than '0'");
        }

        [TestMethod]
        public void Validate_Exporter_AboveMaxSites_ShouldHaveInvalidSiteError()
        {
            var dto = CreateValidDto();
            dto.RequestorType = RequestorTypes.Exporters;
            dto.NumberOfOverseasSites = ReprocessorExporterConstants.MaxNumberOfOverseasSitesAllowed + 1;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.NumberOfOverseasSites));
            Assert.IsNotNull(error);
            Assert.AreEqual(ValidationMessages.InvalidNumberOfOverseasSiteForExporter, error.ErrorMessage);
        }

        [TestMethod]
        public void Validate_Exporter_ValidSites_ShouldNotHaveSiteError()
        {
            var dto = CreateValidDto();
            dto.RequestorType = RequestorTypes.Exporters;
            dto.NumberOfOverseasSites = ReprocessorExporterConstants.MaxNumberOfOverseasSitesAllowed;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.Errors.Any(e => e.PropertyName == nameof(dto.NumberOfOverseasSites)));
        }

        [TestMethod]
        public void Validate_Reprocessor_NonZeroSites_ShouldHaveReprocessorSiteError()
        {
            var dto = CreateValidDto();
            dto.RequestorType = RequestorTypes.Reprocessors;
            dto.NumberOfOverseasSites = 1;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.SingleOrDefault(e => e.PropertyName == nameof(dto.NumberOfOverseasSites));
            Assert.IsNotNull(error);
            Assert.AreEqual(ValidationMessages.InvalidNumberOfOverseasSiteForReprocessor, error.ErrorMessage);
        }

        [TestMethod]
        public void Validate_Reprocessor_ZeroSites_ShouldNotHaveSiteError()
        {
            var dto = CreateValidDto();
            dto.RequestorType = RequestorTypes.Reprocessors;
            dto.NumberOfOverseasSites = 0;
            var result = _validator.Validate(dto);
            Assert.IsFalse(result.Errors.Any(e => e.PropertyName == nameof(dto.NumberOfOverseasSites)));
        }

        [TestMethod]
        public void Validate_RegulatorNull_ShouldHaveRequiredError()
        {
            var dto = CreateValidDto();
            dto.Regulator = null;

            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e =>
                e.PropertyName == nameof(dto.Regulator) &&
                e.ErrorMessage == ValidationMessages.RegulatorRequired));
        }

        [TestMethod]
        public void Validate_SubmissionDateDefault_ShouldHaveError()
        {
            var dto = CreateValidDto();
            dto.SubmissionDate = default;  // DateTime.MinValue

            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid, "Expected an error when SubmissionDate is default(DateTime)");
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(dto.SubmissionDate)));
        }

        [TestMethod]
        public void Validate_SubmissionDateJustBeforeNow_ShouldBeValid()
        {
            var dto = CreateValidDto();
            dto.SubmissionDate = DateTime.UtcNow.AddMilliseconds(-1);

            var result = _validator.Validate(dto);
            Assert.IsTrue(result.IsValid, "SubmissionDate just before now should be allowed");
        }


        [TestMethod]
        public void Validate_Exporter_NegativeSites_ShouldHaveGreaterThanZeroError()
        {
            var dto = CreateValidDto();
            dto.RequestorType = RequestorTypes.Exporters;
            dto.NumberOfOverseasSites = -3;

            var result = _validator.Validate(dto);
            Assert.IsFalse(result.IsValid);
            var error = result.Errors.Single(e => e.PropertyName == nameof(dto.NumberOfOverseasSites));
            StringAssert.Contains(error.ErrorMessage, "must be greater than '0'");
        }

        [TestMethod]
        public void Validate_InvalidRequestorType_NoSiteValidationErrors()
        {
            var dto = CreateValidDto();
            dto.RequestorType = (RequestorTypes)999;
            dto.NumberOfOverseasSites = ReprocessorExporterConstants.MaxNumberOfOverseasSitesAllowed + 5;

            var result = _validator.Validate(dto);

            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(dto.RequestorType)),
                "Expected an error for invalid RequestorType");

            Assert.IsFalse(result.Errors.Any(e => e.PropertyName == nameof(dto.NumberOfOverseasSites)),
                "Did not expect any NumberOfOverseasSites errors for invalid RequestorType");
        }

    }
}