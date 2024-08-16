using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using System.Reflection;

namespace EPR.Payment.Facade.Helpers
{
    public class ConditionalEndpointMiddleware
    {
        private readonly RequestDelegate _next = null!;
        private readonly IFeatureManager _featureManager = null!;
        private readonly ILogger<ConditionalEndpointMiddleware> _logger = null!;

        public ConditionalEndpointMiddleware(RequestDelegate next, IFeatureManager featureManager, ILogger<ConditionalEndpointMiddleware> logger)
        {
            _next = next;
            _featureManager = featureManager;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor != null)
                {
                    _logger.LogInformation("Evaluating feature gate for {ControllerName}.{ActionName}", 
                        controllerActionDescriptor.ControllerName, controllerActionDescriptor.ActionName);

                    var featureAttributes = controllerActionDescriptor.ControllerTypeInfo
                        .GetCustomAttributes<FeatureGateAttribute>(true)
                        .Union(controllerActionDescriptor.MethodInfo.GetCustomAttributes<FeatureGateAttribute>(true))
                        .ToList();

                    foreach (var featureAttribute in featureAttributes)
                    {
                        foreach (var featureName in featureAttribute.Features)
                        {
                            var isEnabled = await _featureManager.IsEnabledAsync(featureName);
                            _logger.LogInformation("Feature '{FeatureName}' is enabled: {IsEnabled}", featureName, isEnabled);

                            if (!isEnabled)
                            {
                                _logger.LogInformation("Feature '{FeatureName}' is disabled. Returning 404.", featureName);
                                context.Response.StatusCode = StatusCodes.Status404NotFound;
                                await context.Response.WriteAsync("Feature not available.");
                                return;
                            }
                        }
                    }
                }
            }

            await _next(context);
        }

    }
}
