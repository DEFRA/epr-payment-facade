namespace EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces
{
    public interface IHttpPaymentServiceHealthCheckService
    {
        Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken);
    }
}
