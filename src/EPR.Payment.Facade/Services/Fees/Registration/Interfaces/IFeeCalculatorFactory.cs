using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Services.Fees.Registration.Interfaces;
using System.Security.AccessControl;

public interface IFeeCalculatorFactory
{
    IFeeCalculatorService<TRequest> GetFeeCalculator<TRequest>();
}