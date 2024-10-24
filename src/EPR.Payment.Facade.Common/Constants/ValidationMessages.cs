namespace EPR.Payment.Facade.Common.Constants
{
    public static class ValidationMessages
    {
        // Producer Registration Fee Validation Messages
        public const string ProducerTypeInvalid = "Producer Type must be one of the following: ";
        public const string NumberOfSubsidiariesRange = "Number of subsidiaries must be greater than or equal to 0.";
        public const string NumberOfOMPSubsidiariesLessThanOrEqualToNumberOfSubsidiaries = "Number of online marketplace subsidiaries must be less than or equal to number of subsidiaries";
        public const string ApplicationReferenceNumberRequired = "Application Reference Number is required.";
        public const string NoOfSubsidiariesOnlineMarketplaceRange = "Number of Subsidiaries with Online Marketplace must be greater than or equal to 0";

        // PaymentRequestDto Validation Messages
        public const string UserIdRequired = "User ID is required.";
        public const string OrganisationIdRequired = "Organisation ID is required.";
        public const string ReferenceRequired = "Reference is required.";
        public const string AmountRequiredAndGreaterThanZero = "Amount is required and must be greater than zero.";
        public const string RegulatorInvalid = "Invalid Regulator.";
        public const string RegulatorNotENG = "Online payment is not supported for this regulator.";

        public const string OfflinePaymentUserIdRequired = "User ID is required.";
        public const string OfflinePaymentReferenceRequired = "Reference is required.";
        public const string OfflinePaymentAmountRequired = "Amount is required.";

        // Common Validation Messages
        public const string RegulatorRequired = "Regulator is required.";
        public const string InvalidComplianceSchemeMember = "Invalid ComplianceSchemeMember entry.";
        public const string InvalidMemberId = "MemberId is required.";
        public const string MemberTypeRequired = "MemberType is required.";
        public const string InvalidMemberType = "Member Type must be one of the following: ";
    }
}