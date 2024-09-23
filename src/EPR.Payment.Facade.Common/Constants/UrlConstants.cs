namespace EPR.Payment.Facade.Common.Constants
{
    public static class UrlConstants
    {
        // Payments service endpoints
        public const string PaymentsInsert = "payments";
        public const string PaymentsUpdate = "payments/{externalPaymentId}";
        public const string GetPaymentDetails = "payments/{externalPaymentId}";

        // GovPay service endpoints
        public const string GovPayInitiatePayment = "payments";
        public const string GovPayGetPaymentStatus = "payments/{paymentId}";

        // Registration fees service endpoints
        public const string CalculateProducerRegistrationFees = "producer/registration-fees";
        public const string GetProducerResubmissionFee = "producer/resubmission-fee?Regulator={regulator}";

        // Compliance scheme fees service endpoints
        public const string GetComplianceSchemeBaseFee = "compliance-scheme/registration-fee?regulator={regulator}";
    }
}