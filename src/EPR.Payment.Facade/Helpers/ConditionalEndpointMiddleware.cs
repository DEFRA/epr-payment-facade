using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using System.Reflection;

namespace EPR.Payment.Facade.Helpers;

public class ConditionalEndpointMiddleware(RequestDelegate next, IFeatureManager featureManager, ILogger<ConditionalEndpointMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (controllerActionDescriptor != null)
        {
            logger.LogInformation("Evaluating feature gate for {ControllerName}.{ActionName}",
                controllerActionDescriptor.ControllerName, controllerActionDescriptor.ActionName);

            var featureAttributes = controllerActionDescriptor.ControllerTypeInfo
                .GetCustomAttributes<FeatureGateAttribute>(true)
                .Union(controllerActionDescriptor.MethodInfo.GetCustomAttributes<FeatureGateAttribute>(true))
                .ToList();

            foreach (var featureName in featureAttributes.SelectMany(featureAttribute => featureAttribute.Features))
            {
                var isEnabled = await featureManager.IsEnabledAsync(featureName);
                logger.LogInformation("Feature '{FeatureName}' is enabled: {IsEnabled}", featureName, isEnabled);

                if (isEnabled) continue;
                logger.LogInformation("Feature '{FeatureName}' is disabled. Returning 404.", featureName);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Feature not available.");
                return;
            }
        }

        await next(context);
    }

}