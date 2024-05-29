using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpGovPayClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
