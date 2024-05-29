using EPR.Payment.Facade.Services.Fees.Registration.Interfaces;

namespace EPR.Payment.Facade.Services.Fees.Registration
{
    public class FeeCalculatorFactory : IFeeCalculatorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FeeCalculatorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFeeCalculatorService<TRequest> GetFeeCalculator<TRequest>() where TRequest : class
        {
            return _serviceProvider.GetRequiredService<IFeeCalculatorService<TRequest>>();
        }
    }
}
