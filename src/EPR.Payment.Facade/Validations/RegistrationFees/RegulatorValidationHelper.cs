using EPR.Payment.Facade.Common.Constants;

namespace EPR.Payment.Facade.Validations.RegistrationFees
{
    public static class RegulatorValidationHelper
    {
        private static readonly List<string> ValidRegulators = new List<string>
        {
            RegulatorConstants.GBENG,
            RegulatorConstants.GBSCT,
            RegulatorConstants.GBWLS,
            RegulatorConstants.GBNIR
        };

        public static bool IsValidRegulator(string regulator)
        {
            return ValidRegulators.Contains(regulator);
        }
    }
}
