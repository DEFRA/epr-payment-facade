using EPR.Payment.Facade.Common.Dtos.Response.Fees;

namespace EPR.Payment.Facade.Services.Fees.Registration.Interfaces
{
    public interface IFeeCalculatorService<TRequest>
    {
        Task<FeeResponse> CalculateFeeAsync(TRequest request);
    }
}
