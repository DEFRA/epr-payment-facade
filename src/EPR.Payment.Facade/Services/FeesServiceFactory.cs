using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Services.Interfaces;

namespace EPR.Payment.Facade.Services
{
    public class FeesServiceFactory : IFeesServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FeesServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFeesService CreateFeesService(RegistrationFeeSubType subType)
        {
            return subType switch
            {
                RegistrationFeeSubType.SmallAndLargeProducers => _serviceProvider.GetRequiredService<SmallAndLargeProducersService>(),
                RegistrationFeeSubType.Subsidaries => _serviceProvider.GetRequiredService<SubsidariesService>(),
                //RegistrationFeeSubType.ResubmissionAndAdditionalFees => _serviceProvider.GetRequiredService<ResubmissionAndAdditionalFeesService>(),
                //RegistrationFeeSubType.ComplianceSchemes => _serviceProvider.GetRequiredService<ComplianceSchemesService>(),
                //RegistrationFeeSubType.ComplianceSchemePlus => _serviceProvider.GetRequiredService<ComplianceSchemePlusService>(),
                //RegistrationFeeSubType.ComplianceSchemeResubmissionAndAdditionalFees => _serviceProvider.GetRequiredService<ComplianceSchemeResubmissionAndAdditionalFeesService>(),
                _ => throw new ArgumentException("Invalid subtype", nameof(subType))
            };
        }
    }
}
