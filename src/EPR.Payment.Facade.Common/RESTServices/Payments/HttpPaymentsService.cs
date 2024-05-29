using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpPaymentsService : IHttpPaymentsService
    {
        // TODO - PS : Need to call the apis within the Payment controller within the EPR.Payment.Service here
        public Task InsertPaymentStatusAsync(string paymentId, PaymentStatusInsertRequestDto paymentStatusUpdateRequest)
        {
            throw new NotImplementedException();
        }
    }
}
