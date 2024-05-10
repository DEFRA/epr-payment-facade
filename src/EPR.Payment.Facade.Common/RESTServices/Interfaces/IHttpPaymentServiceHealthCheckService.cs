namespace EPR.Payment.Facade.Common.RESTServices.Interfaces
{
    public interface IHttpPaymentServiceHealthCheckService
    {
        Task<HttpResponseMessage> GetHealth(CancellationToken cancellationToken);
    }
}
