namespace EPR.Payment.Facade.Common.Constants
{
    public static class LogMessages
    {
        // General Log Messages
        public const string ValidationErrorOccured = "Validation error occurred while processing {0} request";
        public const string ErrorOccured = "An error occurred while processing {0} request";

        // Log Messages for PaymentsService
        public const string ValidationErrorUpdatingPayment = "Validation error occurred while updating payment status.";
        public const string UnexpectedErrorUpdatingPayment = "An unexpected error occurred while updating payment status.";
        public const string ValidationErrorInsertingPayment = "Validation error occurred while inserting payment.";
        public const string UnexpectedErrorInsertingPayment = "An unexpected error occurred while inserting payment.";

        // Log Messages for PaymentsController
        public const string NextUrlNull = "Next URL is null.";
    }
}