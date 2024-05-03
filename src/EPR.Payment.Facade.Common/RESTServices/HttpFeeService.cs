using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices
{
    public class HttpFeeService : IHttpFeeService
    {
        // TODO - PS : Need to call the apis within the Payment Service here
        public Task<GetFeeResponseDto> GetFee(bool isLarge, string regulator)
        {
            throw new NotImplementedException();
        }

        public Task InsertPaymentStatus(string paymentId, PaymentStatusInsertRequestDto paymentStatusUpdateRequest)
        {
            throw new NotImplementedException();
        }
    }
}
