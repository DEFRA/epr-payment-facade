﻿using System.Runtime.Serialization;

namespace EPR.Payment.Facade.Common.Constants
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
        public const string OnlinePaymentServiceBaseUrlMissing = "OnlinePaymentService BaseUrl configuration is missing";
        public const string OnlinePaymentServiceEndPointNameMissing = "OnlinePaymentService EndPointName configuration is missing";
        public const string OnlinePaymentServiceHttpClientNameMissing = "OnlinePaymentService HttpClientName configuration is missing";
        public const string ErrorInsertingOnlinePayment = "Error occurred while inserting payment status.";
        public const string ErrorUpdatingOnlinePayment = "Error occurred while updating payment status.";
        public const string UnexpectedErrorInsertingOnlinePayment = "An unexpected error occurred while inserting the payment.";
        public const string UnexpectedErrorUpdatingOnlinePayment = "An unexpected error occurred while updating the payment status.";
        public const string ErrorRetrievingOnlinePaymentDetails = "Error occurred while retrieving payment details.";
        public const string ErrorGettingOnlinePaymentDetails = "Error occurred while getting payment details.";

        // HttpOfflinePaymentsService exceptions
        public const string OfflinePaymentServiceBaseUrlMissing = "OfflinePaymentService BaseUrl configuration is missing";
        public const string OfflinePaymentServiceEndPointNameMissing = "OfflinePaymentService EndPointName configuration is missing";
        public const string ErrorInsertingOfflinePayment = "Error occurred while inserting payment status.";
        public const string UnexpectedErrorInsertingOfflinePayment = "An unexpected error occurred while inserting the offline payment.";

        // PaymentsService exceptions
        public const string ReturnUrlNotConfigured = "ReturnUrl is not configured.";
        public const string DescriptionNotConfigured = "Description is not configured.";
        public const string GovPayPaymentIdNull = "GovPayPaymentId cannot be null or empty";
        public const string SuccessStatusWithErrorCode = "Error code should be null or empty for a success status.";
        public const string FailedStatusWithoutErrorCode = "Error code cannot be null or empty for a failed status.";
        public const string ErrorStatusWithoutErrorCode = "Error code cannot be null or empty for an error status.";


        // BaseHttpService exceptions
        public const string ApiResponseError = "Error occurred calling API with error code: {0}. Message: {1}";

        // PaymentsController validation messages
        public const string AmountMustBeGreaterThanZero = "Amount must be greater than 0";

        // HttpRegistrationFeesService exceptions
        public const string RegistrationFeesServiceBaseUrlMissing = "RegistrationFeesService BaseUrl configuration is missing";
        public const string RegistrationFeesServiceEndPointNameMissing = "RegistrationFeesService EndPointName configuration is missing";
        public const string RegistrationFeesServiceHttpClientNameMissing = "RegistrationFeesService HttpClientName configuration is missing";
        public const string ErrorCalculatingProducerFees = "Error occurred while calculating producer registration fees.";
        public const string UnexpectedErrorCalculatingProducerFees = "An unexpected error occurred while calculating producer registration fees.";
        public const string ErrorResubmissionFees = "Error occurred while getting resubmission fee.";

        // ProducerFeesService exceptions
        public const string RegulatorCanNotBeNullOrEmpty = "regulator cannot be null or empty";

        // ProducersFeesController specific exceptions
        public const string UnexpectedErrorCalculatingFees = "An unexpected error occurred while calculating the fees.";

        // ComplianceSchemeFeesService exceptions
        public const string ComplianceSchemeServiceUrlMissing = "ComplianceSchemeService url configuration is missing.";
        public const string ComplianceSchemeServiceEndPointNameMissing = "ComplianceSchemeService EndPointName configuration is missing.";
        public const string ComplianceSchemeServiceHttpClientNameMissing = "ComplianceSchemeService HttpClientName configuration is missing.";
        public const string ErrorCalculatingComplianceSchemeFees = "Error occurred while calculating Compliance fees.";
        public const string UnexpectedErrorCalculatingComplianceSchemeFees = "An unexpected error occurred while calculating Compliance Scheme fees.";

    }
}