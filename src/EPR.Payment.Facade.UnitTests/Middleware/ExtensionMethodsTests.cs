using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces;
using EPR.Payment.Facade.Helpers;
using EPR.Payment.Facade.Services.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace EPR.Payment.Facade.UnitTests.Helpers
{
    [TestClass]
    public class ExtensionMethodsTests
    {
        private IServiceCollection? _services;

        [TestInitialize]
        public void Setup()
        {
            _services = new ServiceCollection();

            // Add required services
            _services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _services.AddHttpClient(); // Register the default HttpClientFactory

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;
        }

        [TestMethod]
        public void AddFacadeDependencies_RegistersServicesCorrectly()
        {
            // Arrange
            var configurationData = new Dictionary<string, string>
            {
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.Url)}", "https://payment.service" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.EndPointName)}", "payment" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.GovPayService)}:{nameof(Service.Url)}", "https://govpay.service" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.GovPayService)}:{nameof(Service.EndPointName)}", "govpay" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.GovPayService)}:{nameof(Service.HttpClientName)}", "HttpClient" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.GovPayService)}:{nameof(Service.BearerToken)}", "BearerTokenValue" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.GovPayService)}:{nameof(Service.Retries)}", "3" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ProducerFeesService)}:{nameof(Service.Url)}", "https://producer.fees.service" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ProducerFeesService)}:{nameof(Service.EndPointName)}", "fees" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ProducerFeesService)}:{nameof(Service.HttpClientName)}", "ProducerFeesClient" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ComplianceSchemeFeesService)}:{nameof(Service.Url)}", "https://compliancescheme.fees.service" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ComplianceSchemeFeesService)}:{nameof(Service.EndPointName)}", "fees" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ComplianceSchemeFeesService)}:{nameof(Service.HttpClientName)}", "ComplianceFeesClient" }
            };

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData!)
                .Build();

            // Act
            _services?.AddFacadeDependencies(configurationBuilder);
            var serviceProvider = _services?.BuildServiceProvider();

            // Assert
            using (new AssertionScope())
            {
                var paymentHealthService = serviceProvider?.GetService<IPaymentServiceHealthService>();
                paymentHealthService.Should().NotBeNull();
                paymentHealthService.Should().BeOfType<PaymentServiceHealthService>();

                var httpPaymentServiceHealthCheck = serviceProvider?.GetService<IHttpPaymentServiceHealthCheckService>();
                httpPaymentServiceHealthCheck.Should().NotBeNull();
                httpPaymentServiceHealthCheck.Should().BeOfType<HttpPaymentServiceHealthCheckService>();

                var httpPaymentsService = serviceProvider?.GetService<IHttpPaymentsService>();
                httpPaymentsService.Should().NotBeNull();
                httpPaymentsService.Should().BeOfType<HttpPaymentsService>();

                var httpGovPayService = serviceProvider?.GetService<IHttpGovPayService>();
                httpGovPayService.Should().NotBeNull();
                httpGovPayService.Should().BeOfType<HttpGovPayService>();

                var httpProducerFeesService = serviceProvider?.GetService<IHttpProducerFeesService>();
                httpProducerFeesService.Should().NotBeNull();
                httpProducerFeesService.Should().BeOfType<HttpProducerFeesService>();

                var httpComplianceSchemeFeesService = serviceProvider?.GetService<IHttpComplianceSchemeFeesService>();
                httpComplianceSchemeFeesService.Should().NotBeNull();
                httpComplianceSchemeFeesService.Should().BeOfType<HttpComplianceSchemeFeesService>();
            }
        }

        [TestMethod]
        public void AddFacadeDependencies_WithMissingUrlConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var configurationData = new Dictionary<string, string>
            {
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.Url)}", null! },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.EndPointName)}", "payment" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" }
            };

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData!)
                .Build();

            // Act
            Action act = () => _services?.AddFacadeDependencies(configurationBuilder).BuildServiceProvider();

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("PaymentService Url configuration is missing.");
        }

        [TestMethod]
        public void AddFacadeDependencies_WithMissingEndPointNameConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var configurationData = new Dictionary<string, string>
            {
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.Url)}", "https://payment.service" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.EndPointName)}", null! },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" }
            };

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData!)
                .Build();

            // Act
            Action act = () => _services?.AddFacadeDependencies(configurationBuilder).BuildServiceProvider();

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("PaymentService EndPointName configuration is missing.");
        }
    }
}