namespace EPR.Payment.Facade.Services.Interfaces
{
    public interface IPaymentServiceHealthService
    {
        Task<HttpResponseMessage> GetHealth(CancellationToken cancellationToken);
    }
}