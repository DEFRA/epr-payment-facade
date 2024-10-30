namespace EPR.Payment.Facade.Common.Constants
{
    public static class ValidationMessages
    {
        // Producer Registration Fee Validation Messages
        public const string ProducerTypeInvalid = "Producer Type must be one of the following: ";
        public const string NumberOfSubsidiariesRange = "Number of subsidiaries must be greater than or equal to 0.";
        public const string NumberOfOMPSubsidiariesLessThanOrEqualToNumberOfSubsidiaries = "Number of online marketplace subsidiaries must be less than or equal to number of subsidiaries.";
        public const string ApplicationReferenceNumberRequired = "Application Reference Number is required.";
        public const string NoOfSubsidiariesOnlineMarketplaceRange = "Number of Subsidiaries with Online Marketplace must be greater than or equal to 0.";

        // PaymentRequestDto Validation Messages
        public const string UserIdRequired = "User ID is required.";
        public const string OrganisationIdRequired = "Organisation ID is required.";
        public const string ReferenceRequired = "Reference is required.";
        public const string AmountRequiredAndGreaterThanZero = "Amount is required and must be greater than zero.";
        public const string RegulatorInvalid = "Invalid Regulator.";
        public const string RegulatorNotENG = "Online payment is not supported for this regulator.";
        public const string DescriptionRequired = "Description is required.";

        // Common Validation Messages
        public const string RegulatorRequired = "Regulator is required.";
        public const string InvalidComplianceSchemeMember = "Invalid ComplianceSchemeMember entry.";
        public const string InvalidMemberId = "MemberId is required.";
        public const string MemberTypeRequired = "MemberType is required.";
        public const string InvalidMemberType = "Member Type must be one of the following: ";
        public const string InvalidSubmissionDate = "Submission Date is required. It must be a valid UTC date.";
        public const string FutureSubmissionDate = "Submission Date can not be future dated.";

        // Compliance Scheme Resubmission Fee Validation Messages
        public const string ResubmissionDateRequired = "Resubmission Date is required.";
        public const string ResubmissionDateInvalid = "Resubmission Date cannot be in the future.";
        public const string ResubmissionDateMustBeUtc = "Resubmission Date must be in UTC.";
        public const string ResubmissionDateInvalidFormat = "Resubmission Date should be in the format YYYY-MM-DD HH:MM:SS.";
        public const string ResubmissionDateDefaultInvalid = "Resubmission Date must be a valid date.";
        public const string ReferenceNumberRequired = "Reference Number is required.";
        public const string MemberCountGreaterThanZero = "Member Count must be greater than zero.";
    }
}