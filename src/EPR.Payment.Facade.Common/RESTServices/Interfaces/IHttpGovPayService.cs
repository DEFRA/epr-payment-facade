using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Interfaces
{
    public interface IHttpGovPayService
    {
        Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto requestDto);
        Task<PaymentStatusResponseDto> GetPaymentStatusAsync(string paymentId);
    }
}
