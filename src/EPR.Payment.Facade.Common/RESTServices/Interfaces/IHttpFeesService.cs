using EPR.Payment.Facade.Common.Dtos.Request;
using EPR.Payment.Facade.Common.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Interfaces
{
    public interface IHttpFeesService
    {
        Task<GetFeesResponseDto> GetFee(bool isLarge, string regulator);        
    }
}
