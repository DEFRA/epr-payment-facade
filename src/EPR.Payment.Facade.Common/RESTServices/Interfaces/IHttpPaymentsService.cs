using EPR.Payment.Facade.Common.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Interfaces
{
    public interface IHttpPaymentsService
    {
        Task InsertPaymentStatus(string paymentId, PaymentStatusInsertRequestDto paymentStatusUpdateRequest);
    }
}
