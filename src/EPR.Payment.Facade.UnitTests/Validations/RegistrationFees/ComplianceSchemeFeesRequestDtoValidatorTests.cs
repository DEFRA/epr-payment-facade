﻿using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.Dtos.Request.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Validations.RegistrationFees.ComplianceScheme;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.RegistrationFees
{
    [TestClass]
    public class ComplianceSchemeFeesRequestDtoValidatorTests
    {
        private ComplianceSchemeFeesRequestDtoValidator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new ComplianceSchemeFeesRequestDtoValidator();
        }

        [TestMethod]
        public void Validator_Should_Fail_When_Regulator_Is_Empty()
        {
            // Arrange
            var dto = new ComplianceSchemeFeesRequestDto
            {
                Regulator = "",
                ApplicationReferenceNumber = "Ref123",
                SubmissionDate = DateTime.UtcNow,
                ComplianceSchemeMembers = new List<ComplianceSchemeMemberDto>()
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Regulator)
                  .WithErrorMessage(ValidationMessages.RegulatorRequired);
        }

        [TestMethod]
        public void Validator_Should_Fail_When_ApplicationReferenceNumber_Is_Empty()
        {
            // Arrange
            var dto = new ComplianceSchemeFeesRequestDto
            {
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "",
                SubmissionDate = DateTime.UtcNow,
                ComplianceSchemeMembers = new List<ComplianceSchemeMemberDto>()
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ApplicationReferenceNumber)
                  .WithErrorMessage(ValidationMessages.ApplicationReferenceNumberRequired);
        }

        [TestMethod]
        public void Validator_Should_Validate_ComplianceSchemeMember()
        {
            // Arrange
            var dto = new ComplianceSchemeFeesRequestDto
            {
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "Ref123",
                SubmissionDate = DateTime.UtcNow,
                ComplianceSchemeMembers = new List<ComplianceSchemeMemberDto>
                {
                    new ComplianceSchemeMemberDto { MemberId = "123", MemberType = "Large", NumberOfSubsidiaries = 2, NoOfSubsidiariesOnlineMarketplace = 1 },
                    new ComplianceSchemeMemberDto { MemberId = string.Empty, MemberType = "Small", NumberOfSubsidiaries = -1, NoOfSubsidiariesOnlineMarketplace = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.Errors.Should().ContainSingle(e => e.PropertyName == "ComplianceSchemeMembers[1].MemberId" &&
                                   e.ErrorMessage == ValidationMessages.InvalidMemberId);

            result.Errors.Should().ContainSingle(e => e.PropertyName == "ComplianceSchemeMembers[1].NumberOfSubsidiaries" &&
                                   e.ErrorMessage == ValidationMessages.NumberOfSubsidiariesRange);
        }

        [TestMethod]
        public void Validator_Should_Fail_When_SubmissionDate_Is_Not_Valid()
        {
            // Arrange
            var dto = new ComplianceSchemeFeesRequestDto
            {
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "Ref123",
                SubmissionDate = default(DateTime),
                ComplianceSchemeMembers = new List<ComplianceSchemeMemberDto>()
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SubmissionDate)
                  .WithErrorMessage(ValidationMessages.InvalidSubmissionDate);
        }

        [TestMethod]
        public void Validator_Should_Fail_When_SubmissionDate_Is_Future_Date()
        {
            // Arrange
            var dto = new ComplianceSchemeFeesRequestDto
            {
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "Ref123",
                SubmissionDate = DateTime.UtcNow.AddDays(10),
                ComplianceSchemeMembers = new List<ComplianceSchemeMemberDto>()
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SubmissionDate)
                  .WithErrorMessage(ValidationMessages.FutureSubmissionDate);
        }

        [TestMethod]
        public void Validate_SubmissionDateNotUtc_ShouldHaveError()
        {
            // Arrange
            var dto = new ComplianceSchemeFeesRequestDto
            {
                Regulator = RegulatorConstants.GBENG,
                ApplicationReferenceNumber = "Ref123",
                SubmissionDate = DateTime.Now,
                ComplianceSchemeMembers = new List<ComplianceSchemeMemberDto>()
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SubmissionDate)
                .WithErrorMessage(ValidationMessages.SubmissionDateMustBeUtc);
        }
    }
}
