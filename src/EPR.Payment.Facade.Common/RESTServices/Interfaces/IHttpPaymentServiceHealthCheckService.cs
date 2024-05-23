namespace EPR.Payment.Facade.Common.RESTServices.Interfaces
{
    public interface IHttpPaymentServiceHealthCheckService
    {
        Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken);
    }
}
