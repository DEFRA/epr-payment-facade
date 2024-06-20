﻿using System.Threading.Tasks;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;

namespace EPR.Payment.Facade.Services.Payments.Interfaces
{
    public interface IPaymentsService
    {
        Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request);
        Task CompletePaymentAsync(string govPayPaymentId, CompletePaymentRequestDto completeRequest);
    }
}
