using EPR.Payment.Facade.Common.Dtos.Response.Fees;

namespace EPR.Payment.Facade.Services.Fees.Interfaces
{
    public interface IHttpFeeService
    {
        Task<FeeResponse> CalculateFeeAsync(RegistrationFeeRequest request);
    }
}
