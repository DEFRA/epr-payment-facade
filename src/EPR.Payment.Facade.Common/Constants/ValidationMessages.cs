﻿namespace EPR.Payment.Facade.Common.Constants
{
    public static class ValidationMessages
    {
        // Producer Registration Fee Validation Messages
        public const string ProducerTypeInvalid = "ProducerType must be one of the following: ";
        public const string NumberOfSubsidiariesRange = "Number of subsidiaries must be between 0 and 100.";
        public const string NumberOfSubsidiariesRequiredWhenProducerTypeEmpty = "Number of subsidiaries must be greater than 0 when ProducerType is empty.";

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