namespace EPR.Payment.Facade.Services.Interfaces
{
    public interface IPaymentServiceHealthService
    {
        Task<HttpResponseMessage> GetHealthAsync(CancellationToken cancellationToken);
    }
}