using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EPR.Payment.Facade.Helpers
{
    public class FeatureGateOperationFilter : IOperationFilter
    {
        private readonly IFeatureManager _featureManager;

        public FeatureGateOperationFilter(IFeatureManager featureManager)
        {
            _featureManager = featureManager;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var featureGateAttributes = context.MethodInfo.GetCustomAttributes(typeof(FeatureGateAttribute), false) as FeatureGateAttribute[];

            if (featureGateAttributes != null)
            {
                foreach (var featureGateAttribute in featureGateAttributes)
                {
                    foreach (var featureName in featureGateAttribute.Features)
                    {
                        var featureEnabled = _featureManager.IsEnabledAsync(featureName).Result;

                        if (!featureEnabled)
                        {
                            operation.Deprecated = true;
                            operation.Description += " (This feature is currently disabled)";
                        }
                    }
                }
            }
        }
    }
}