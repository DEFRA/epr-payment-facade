using Asp.Versioning;
using EPR.Payment.Common.Mapping;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Extension;
using EPR.Payment.Facade.HealthCheck;
using EPR.Payment.Facade.Helpers;
using EPR.Payment.Facade.Validations.Payments;
using EPR.Payment.Facade.Validations.RegistrationFees.Producer;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Security.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add Azure AD B2C configuration
var azureAdB2CConfig = builder.Configuration.GetSection("AzureAdB2C");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<OnlinePaymentRequestDtoValidator>();
    fv.RegisterValidatorsFromAssemblyContaining<ProducerFeesRequestDtoValidator>();
    fv.AutomaticValidationEnabled = false;
});
builder.Services.Configure<OnlinePaymentServiceOptions>(builder.Configuration.GetSection("PaymentServiceOptions"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction =>
{
    setupAction.EnableAnnotations();
    setupAction.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentFacadeApi", Version = "v1" });
    setupAction.DocumentFilter<FeatureEnabledDocumentFilter>();
    setupAction.OperationFilter<FeatureGateOperationFilter>();

    // Configure OAuth2 for Swagger
    setupAction.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{azureAdB2CConfig["Instance"]}/{azureAdB2CConfig["Domain"]}/{azureAdB2CConfig["SignUpSignInPolicyId"]}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"{azureAdB2CConfig["Instance"]}/{azureAdB2CConfig["Domain"]}/{azureAdB2CConfig["SignUpSignInPolicyId"]}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { azureAdB2CConfig["Scopes"]!, "Access the Payment API" }
                }
            }
        },
        Description = "Authenticate with Azure AD B2C to access this API."
    });

    // Require authentication for all endpoints in Swagger
    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "OAuth2"
                }
            },
            new[] { azureAdB2CConfig["Scopes"] }
        }
    });
});

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpContextAccessor();
builder.Services.AddServiceHealthChecks();
builder.Services.AddHttpClient("HttpClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler()
        {
            SslProtocols = SslProtocols.Tls12
        };
    });
builder.Services.AddFacadeDependencies(builder.Configuration);
builder.Services.AddDependencies();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(PaymentRequestMappingProfile));

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddFeatureManagement();

// Conditional Authentication based on Feature Flag
using var serviceProvider = builder.Services.BuildServiceProvider();
var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();
if (await featureManager.IsEnabledAsync("EnableAuthenticationFeature"))
{
    // Authentication
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(
            options =>
            {
                builder.Configuration.Bind(Constants.AzureAdB2C, options);
            },
            options =>
            {
                builder.Configuration.Bind(Constants.AzureAdB2C, options);
            });


    builder.Services.AddAuthorizationBuilder().AddFallbackPolicy("EprPaymentFallBackPolicy", new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());

}
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

bool enableOnlinePaymentsFeature = await featureManager.IsEnabledAsync("EnableOnlinePaymentsFeature");
bool enablePaymentInitiation = await featureManager.IsEnabledAsync("EnablePaymentInitiation");
bool enablePaymentStatus = await featureManager.IsEnabledAsync("EnablePaymentStatus");
bool enablePaymentStatusInsert = await featureManager.IsEnabledAsync("EnablePaymentStatusInsert");
bool enableHomePage = await featureManager.IsEnabledAsync("EnableHomePage");

logger.LogInformation("EnableOnlinePaymentsFeature: {EnableOnlinePaymentsFeature}", enableOnlinePaymentsFeature);
logger.LogInformation("EnablePaymentInitiation: {EnablePaymentInitiation}", enablePaymentInitiation);
logger.LogInformation("EnablePaymentStatus: {EnablePaymentStatus}", enablePaymentStatus);
logger.LogInformation("EnablePaymentStatusInsert: {EnablePaymentStatusInsert}", enablePaymentStatusInsert);
logger.LogInformation("EnableHomePage: {EnableHomePage}", enableHomePage);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentFacadeApi v1");
        c.RoutePrefix = "swagger";

        // OAuth2 settings for Swagger UI
        c.OAuthUsePkce(); // Use PKCE for secure authentication
        c.OAuthScopeSeparator(" ");
        c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    });
}

// Conditionally serve static files based on the feature flag
if (enableHomePage)
{
    app.UseStaticFiles(); // Enable serving static files only if the feature is enabled
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");

// Conditionally apply Authentication and Authorization
if (await featureManager.IsEnabledAsync("EnableAuthenticationFeature"))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseMiddleware<ConditionalEndpointMiddleware>();

// Check if the homepage is enabled and serve index.html accordingly
app.MapGet("/", async context =>
{
    if (enableHomePage)
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
    }
    else
    {
        context.Response.StatusCode = 404;
    }
});

app.MapControllers();
app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();
await app.RunAsync();
