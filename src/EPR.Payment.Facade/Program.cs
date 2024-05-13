using Asp.Versioning;
using EPR.Payment.Facade.AppStart;
using EPR.Payment.Facade.Extension;
using EPR.Payment.Facade.Helpers;
using System.Reflection;
using System.Security.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction => 
{
    setupAction.EnableAnnotations();
} );
builder.Services.AddHttpContextAccessor();
builder.Services.AddServiceHealthChecks();
builder.Services
    .AddHttpClient("HttpClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler()
        {
            SslProtocols = SslProtocols.Tls12
        };
    });
builder.Services.AddFacadeDependencies(builder.Configuration);

builder.Services.AddDependencies();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI();
//.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentFacadeApi");
//    c.RoutePrefix = string.Empty;
//});

app.UseHttpsRedirection();
app.UseRouting();
app.UseHealthChecks();
app.UseAuthorization();

app.MapControllers();

app.Run();
