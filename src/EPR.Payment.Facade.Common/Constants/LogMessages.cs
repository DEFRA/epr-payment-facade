﻿namespace EPR.Payment.Facade.Common.Constants
{
    public static class LogMessages
    {
        // General Log Messages
        public const string ValidationErrorOccured = "Validation error occurred while processing {MethodName} request";
        // Used in controller when validation fails.

        public const string ErrorOccured = "An error occurred while processing {MethodName} request";
        // General-purpose error log message template.

        // Log Messages for PaymentsService
        public const string ValidationErrorUpdatingPayment = "Validation error occurred while updating payment status.";
        public const string UnexpectedErrorUpdatingPayment = "An unexpected error occurred while updating payment status.";
        public const string ValidationErrorInsertingPayment = "Validation error occurred while inserting payment.";
        public const string UnexpectedErrorInsertingPayment = "An unexpected error occurred while inserting payment.";

        // Log Messages for PaymentsController
        public const string NextUrlNull = "Next URL is null.";

        // Log Messages for RegistrationFeesService
        public const string ErrorOccuredWhileCalculatingProducerFees = "An error occurred while calculating producer fees in {MethodName} request";
        // Used in service and controller when errors occur during producer fees calculation.

        // Log Messages for ComplianceSchemeFeesService
        public const string ErrorOccuredWhileRetrievingComplianceSchemeFees = "An error occurred while retrieving compliance scheme fees in {MethodName} request";
        // Used in service when an error occurs during the retrieval of compliance scheme fees.

        public const string UnexpectedErrorWhileRetrievingComplianceSchemeFees = "An unexpected error occurred while retrieving compliance scheme fees in {MethodName} request";
        // Used in service when an unexpected error occurs during the retrieval of compliance scheme fees.

        // Log Messages for ComplianceSchemeFeesController
        public const string ErrorOccuredWhileCalculatingComplianceSchemeFees = "An error occurred while calculating compliance scheme fees in {MethodName} request";
        // Used in controller when the service throws a ServiceException.

        public const string UnexpectedErrorWhileProcessingComplianceSchemeRequest = "An unexpected error occurred while processing compliance scheme request in {MethodName}";
        // Used in controller when an unexpected error occurs during processing.
    }
}