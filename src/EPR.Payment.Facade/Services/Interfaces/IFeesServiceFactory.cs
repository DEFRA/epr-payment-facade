using EPR.Payment.Facade.Common.Enums;

namespace EPR.Payment.Facade.Services.Interfaces
{
    public interface IFeesServiceFactory
    {
        IFeesService CreateFeesService(RegistrationFeeSubType subType);
    }
}
