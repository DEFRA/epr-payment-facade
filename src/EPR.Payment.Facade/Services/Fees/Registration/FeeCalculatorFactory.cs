using EPR.Payment.Facade.Services.Fees.Registration.Interfaces;

namespace EPR.Payment.Facade.Services.Fees.Registration
{
    public class FeeCalculatorFactory : IFeeCalculatorFactory
    {
        public IFeeCalculatorService<TRequest> GetFeeCalculator<TRequest>()
        {
            // Implement logic to return appropriate calculator based on TRequest
            if (typeof(TRequest) == typeof(RegistrationFeeRequest))
            {
                return (IFeeCalculatorService<TRequest>)new RegistrationFeeCalculatorService();
            }
            //else if (typeof(TRequest) == typeof(AccreditationFeeRequest))
            //{
            //    return (IFeeCalculatorService<TRequest>)new AccreditationFeeCalculatorService();
            //}
            else
            {
                throw new ArgumentException("Unsupported fee request type");
            }
        }
    }


}
