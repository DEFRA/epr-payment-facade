namespace EPR.Payment.Facade.Common.Constants
{
    public static class ValidationMessages
    {
        // Producer Registration Fee Validation Messages
        public const string ProducerTypeRequired = "ProducerType is required.";
        public const string ProducerTypeInvalid = "ProducerType must be 'L' for Large or 'S' for Small.";
        public const string NumberOfSubsidiariesRange = "Number of subsidiaries must be between 0 and 100.";

        // PaymentRequestDto Validation Messages
        public const string UserIdRequired = "User ID is required.";
        public const string OrganisationIdRequired = "Organisation ID is required.";
        public const string ReferenceRequired = "Reference is required.";
        public const string AmountRequiredAndGreaterThanZero = "Amount is required and must be greater than zero.";
        public const string RegulatorInvalid = "Invalid Regulator.";
        public const string RegulatorNotENG = "Online payment is not supported for this regulator.";

        // Common Validation Messages
        public const string RegulatorRequired = "Regulator is required.";
    }
}
