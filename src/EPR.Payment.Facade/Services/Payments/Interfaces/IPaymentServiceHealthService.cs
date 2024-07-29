namespace EPR.Payment.Facade.Services.Payments.Interfaces
{
    public interface IPaymentServiceHealthService
    {
        Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken);
    }
}