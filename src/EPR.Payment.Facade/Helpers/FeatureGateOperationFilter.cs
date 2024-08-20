using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace EPR.Payment.Facade.Helpers
{
    public class FeatureGateOperationFilter(IFeatureManager featureManager) : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.GetCustomAttributes(typeof(FeatureGateAttribute), false) is not FeatureGateAttribute
                [] featureGateAttributes) return;
            foreach (var featureGateAttribute in featureGateAttributes)
            {
                foreach (var featureName in featureGateAttribute.Features)
                {
                    var featureEnabled = featureManager.IsEnabledAsync(featureName).Result;

                    if (featureEnabled) continue;
                    operation.Deprecated = true;
                    operation.Description = new StringBuilder(operation.Description).Append("(This feature is currently disabled)").ToString();
                }
            }
        }
    }
}