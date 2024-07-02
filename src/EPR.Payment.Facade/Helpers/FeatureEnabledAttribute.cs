namespace EPR.Payment.Facade.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class FeatureEnabledAttribute : Attribute
    {
        public string FeatureName { get; }

        public FeatureEnabledAttribute(string featureName)
        {
            FeatureName = featureName;
        }
    }
}
