using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Payments
{
    public class HttpFeesService : IHttpFeesService
    {
        // TODO - PS : Need to call the apis within the Fee controller within the EPR.Payment.Service here
        public Task<GetFeesResponseDto> GetFeeAsync(bool isLarge, string regulator)
        {
            throw new NotImplementedException();
        }
    }
}
