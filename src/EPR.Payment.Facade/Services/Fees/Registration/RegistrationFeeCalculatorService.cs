using EPR.Payment.Facade.Common.Dtos.Response.Fees;
using EPR.Payment.Facade.Services.Fees.Registration.Interfaces;

namespace EPR.Payment.Facade.Services.Fees.Registration
{
    public class RegistrationFeeCalculatorService : IFeeCalculatorService<RegistrationFeeRequest>
    {
        public async Task<FeeResponse> CalculateFeeAsync(RegistrationFeeRequest request)
        {
            decimal baseFee = 0;// _feeRepository.GetBaseFee("Registration");

            //if (request.Subsidiaries > 0 && request.Subsidiaries <= 20)
            //{
            //    baseFee *= 1.1m;
            //}
            //else if (request.Subsidiaries > 20)
            //{
            //    baseFee *= 1.2m;
            //}

            //if (request.IsLate)
            //{
            //    baseFee *= 1.25m;
            //}

            //if (request.IsResubmission)
            //{
            //    baseFee += _feeRepository.GetResubmissionFee();
            //}

            //if (request.IsOnlineMarketplace)
            //{
            //    baseFee += _feeRepository.GetOnlineMarketplaceFee();
            //}

            return new FeeResponse { Fee = baseFee };
        }
    }
}
