﻿namespace EPR.Payment.Facade.Common.Constants
{
    public static class ExceptionMessages
    {
        // HttpGovPayService exceptions
        public const string BearerTokenNull = "Bearer token is null. Unable to initiate payment.";
        public const string GovPayResponseInvalid = "GovPay response does not contain a valid PaymentId.";
        public const string PaymentStatusNotFound = "Payment status not found or status is not available.";
        public const string ErrorInitiatingPayment = "Error occurred while initiating payment.";
        public const string ErrorRetrievingPaymentStatus = "Error occurred while retrieving payment status.";

        // HttpPaymentsService exceptions
        public const string PaymentServiceBaseUrlMissing = "PaymentService BaseUrl configuration is missing";
        public const string PaymentServiceEndPointNameMissing = "PaymentService EndPointName configuration is missing";
        public const string PaymentServiceHttpClientNameMissing = "PaymentService HttpClientName configuration is missing";
        public const string ErrorInsertingPayment = "Error occurred while inserting payment status.";
        public const string ErrorUpdatingPayment = "Error occurred while updating payment status.";
        public const string UnexpectedErrorInsertingPayment = "An unexpected error occurred while inserting the payment.";
        public const string UnexpectedErrorUpdatingPayment = "An unexpected error occurred while updating the payment status.";

        // PaymentsService exceptions
        public const string ReturnUrlNotConfigured = "ReturnUrl is not configured.";
        public const string DescriptionNotConfigured = "Description is not configured.";
        public const string GovPayPaymentIdNull = "GovPayPaymentId cannot be null or empty";

        // BaseHttpService exceptions
        public const string ApiResponseError = "Error occurred calling API with error code: {0}. Message: {1}";
    }
}