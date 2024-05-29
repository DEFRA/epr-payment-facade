using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpPaymentsService
    {
        Task InsertPaymentStatusAsync(string paymentId, PaymentStatusInsertRequestDto paymentStatusUpdateRequest);
    }
}
