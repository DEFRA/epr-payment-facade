namespace EPR.Payment.Facade.Common.Constants
{
    public static class UrlConstants
    {
        // Payments service endpoints
        public const string OnlinePaymentsInsert = "onlinepayments";
        public const string OnlinePaymentsUpdate = "onlinepayments/{externalPaymentId}";
        public const string GetOnlinePaymentDetails = "onlinepayments/{externalPaymentId}";

        // GovPay service endpoints
        public const string GovPayInitiatePayment = "payments";
        public const string GovPayGetPaymentStatus = "payments/{paymentId}";

        // Registration fees service endpoints
        public const string CalculateProducerRegistrationFees = "producer/registration-fee";
        public const string GetProducerResubmissionFee = "producer/resubmission-fee?Regulator={regulator}";

        // Compliance scheme fees service endpoints
        public const string CalculateComplianceSchemeFee = "compliance-scheme/registration-fee";
    }
}