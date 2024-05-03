using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Interfaces
{
    public interface IHttpFeeService
    {
        Task<GetFeeResponseDto> GetFee(bool isLarge, string regulator);
        Task InsertPaymentStatus(string paymentId, PaymentStatusInsertRequestDto paymentStatusUpdateRequest);
    }
}
