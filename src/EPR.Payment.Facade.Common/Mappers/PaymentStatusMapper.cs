using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Exceptions;

namespace EPR.Payment.Facade.Common.Mappers
{
    public static class PaymentStatusMapper
    {
        public static PaymentStatus GetPaymentStatus(string status, string? errorCode)
        {
            switch (status.ToLower())
            {
                case "success":
                    if (!string.IsNullOrEmpty(errorCode))
                    {
                        throw new ServiceException(ExceptionMessages.SuccessStatusWithErrorCode);
                    }
                    return PaymentStatus.Success;
                case "failed":
                    if (string.IsNullOrEmpty(errorCode))
                    {
                        throw new ServiceException(ExceptionMessages.FailedStatusWithoutErrorCode);
                    }
                    return errorCode switch
                    {
                        "P0030" => PaymentStatus.UserCancelled,
                        _ => PaymentStatus.Failed,
                    };
                case "error":
                    if (string.IsNullOrEmpty(errorCode))
                    {
                        throw new ServiceException(ExceptionMessages.ErrorStatusWithoutErrorCode);
                    }
                    return PaymentStatus.Error;
                default:
                    throw new ServiceException(ExceptionMessages.PaymentStatusNotFound);
            }
        }
    }
}
