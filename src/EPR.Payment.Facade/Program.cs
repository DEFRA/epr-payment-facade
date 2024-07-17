using Asp.Versioning;
using EPR.Payment.Common.Mapping;
using EPR.Payment.Facade.AppStart;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Extension;
using EPR.Payment.Facade.Helpers;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using System.Security.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<PaymentServiceOptions>(builder.Configuration.GetSection("PaymentServiceOptions"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction =>
{
    setupAction.EnableAnnotations();
    setupAction.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentFacadeApi", Version = "v1" });
    setupAction.DocumentFilter<FeatureEnabledDocumentFilter>();
    setupAction.OperationFilter<FeatureGateOperationFilter>();
});

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
builder.Services.AddLogging();

var app = builder.Build();

var featureManager = app.Services.GetRequiredService<IFeatureManager>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

bool enablePaymentsFeature = await featureManager.IsEnabledAsync("EnablePaymentsFeature");
bool enablePaymentInitiation = await featureManager.IsEnabledAsync("EnablePaymentInitiation");
bool enablePaymentStatus = await featureManager.IsEnabledAsync("EnablePaymentStatus");
bool enablePaymentStatusInsert = await featureManager.IsEnabledAsync("EnablePaymentStatusInsert");

logger.LogInformation($"EnablePaymentsFeature: {enablePaymentsFeature}");
logger.LogInformation($"EnablePaymentInitiation: {enablePaymentInitiation}");
logger.LogInformation($"EnablePaymentStatus: {enablePaymentStatus}");
logger.LogInformation($"EnablePaymentStatusInsert: {enablePaymentStatusInsert}");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentFacadeApi v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable serving static files
app.UseRouting();
app.UseCors("AllowAll");
app.UseHealthChecks();
app.UseAuthorization();
app.UseMiddleware<ConditionalEndpointMiddleware>();

app.MapControllers();

app.Run();
