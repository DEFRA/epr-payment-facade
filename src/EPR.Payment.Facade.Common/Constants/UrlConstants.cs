namespace EPR.Payment.Facade.Common.Constants
{
    public static class UrlConstants
    {
        // Payments service endpoints
        public const string OnlinePaymentsInsert = "online-payments";
        public const string OnlinePaymentsUpdate = "online-payments/{externalPaymentId}";
        public const string GetOnlinePaymentDetails = "online-payments/{externalPaymentId}";

        // Payments service endpoints
        public const string OfflinePaymentsInsert = "offline-payments";

        // GovPay service endpoints
        public const string GovPayInitiatePayment = "payments";
        public const string GovPayGetPaymentStatus = "payments/{paymentId}";

        // Registration fees service endpoints
        public const string CalculateProducerRegistrationFees = "producer/registration-fee";
        public const string GetProducerResubmissionFee = "producer/resubmission-fee";

        // Compliance scheme fees service endpoints
        public const string CalculateComplianceSchemeFee = "compliance-scheme/registration-fee";

        // Compliance scheme resubmission fee endpoints
        public const string GetComplianceSchemeResubmissionFee = "compliance-scheme/resubmission-fee";
    }
}