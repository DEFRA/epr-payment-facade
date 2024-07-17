namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpGovPayClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
