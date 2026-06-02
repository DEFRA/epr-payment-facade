using EPR.Payment.Facade.Common.Dtos.Request.RegistrationSubmission;
using EPR.Payment.Facade.Validations.RegistrationSubmission;
using FluentAssertions;

namespace EPR.Payment.Facade.UnitTests.Validations.RegistrationSubmission
{
    [TestClass]
    public class CreateRegistrationSubmissionDataRequestValidatorTests
    {
        private readonly CreateRegistrationSubmissionDataRequestValidator _sut = new();

        [TestMethod]
        public void Validate_AllFieldsPopulated_Passes()
        {
            var request = new CreateRegistrationSubmissionDataRequest
            {
                SubmissionId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                RegistrationBlobName = "av-blob-name",
                ComplianceSchemeId = Guid.NewGuid(),
                SubmissionPeriod = "Jan to Jun 2026",
                SubmissionDate = new DateTime(2026, 5, 28, 0, 0, 0, DateTimeKind.Utc),
            };

            _sut.Validate(request).IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_AllRequiredMissing_FailsEachField()
        {
            var request = new CreateRegistrationSubmissionDataRequest
            {
                SubmissionId = Guid.Empty,
                FileId = Guid.Empty,
                RegistrationBlobName = string.Empty,
                SubmissionPeriod = string.Empty,
                SubmissionDate = default,
            };

            var result = _sut.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Select(e => e.PropertyName).Should().Contain(new[] { "SubmissionId", "FileId", "RegistrationBlobName", "SubmissionPeriod", "SubmissionDate" });
        }

        [TestMethod]
        public void Validate_ComplianceSchemeIdOptional()
        {
            var request = new CreateRegistrationSubmissionDataRequest
            {
                SubmissionId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                RegistrationBlobName = "av-blob-name",
                ComplianceSchemeId = null,
                SubmissionPeriod = "Jan to Jun 2026",
                SubmissionDate = new DateTime(2026, 5, 28, 0, 0, 0, DateTimeKind.Utc),
            };

            _sut.Validate(request).IsValid.Should().BeTrue();
        }
    }
}
