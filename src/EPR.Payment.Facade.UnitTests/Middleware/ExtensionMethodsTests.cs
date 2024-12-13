using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces;
using EPR.Payment.Facade.Helpers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ProducerFeesService)}:{nameof(Service.ServiceClientId)}", "ServiceClientId" },
        { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.Url)}", "https://payment.service" },
        { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.EndPointName)}", "payment" },
        { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" },
        { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.OfflinePaymentService)}:{nameof(Service.Url)}", "https://offline-payment.service" },
        { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.OfflinePaymentService)}:{nameof(Service.EndPointName)}", "offline-payment" },
        { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.OfflinePaymentService)}:{nameof(Service.HttpClientName)}", "offline-paymentHttpClient" },
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
                .AddInMemoryCollection(configurationData)
                .Build();

            _services?.AddFacadeDependencies(configurationBuilder);
            var serviceProvider = _services?.BuildServiceProvider();

            using (new AssertionScope())
            {
                // Verify configuration
                var optionsMonitor = serviceProvider?.GetService<IOptionsMonitor<Service>>();
                optionsMonitor.Should().NotBeNull();

                var paymentServiceConfig = optionsMonitor!.Get("PaymentService");
                paymentServiceConfig.Should().NotBeNull();
                paymentServiceConfig.Url.Should().Be("https://payment.service");
                paymentServiceConfig.EndPointName.Should().Be("payment");

                var offlinePaymentServiceConfig = optionsMonitor.Get("OfflinePaymentService");
                offlinePaymentServiceConfig.Should().NotBeNull();
                offlinePaymentServiceConfig.Url.Should().Be("https://offline-payment.service");
                offlinePaymentServiceConfig.EndPointName.Should().Be("offline-payment");

                var govPayServiceConfig = optionsMonitor.Get("GovPayService");
                govPayServiceConfig.Should().NotBeNull();
                govPayServiceConfig.Url.Should().Be("https://govpay.service");
                govPayServiceConfig.EndPointName.Should().Be("govpay");

                var producerFeesServiceConfig = optionsMonitor.Get("ProducerFeesService");
                producerFeesServiceConfig.Should().NotBeNull();
                producerFeesServiceConfig.Url.Should().Be("https://producer.fees.service");
                producerFeesServiceConfig.EndPointName.Should().Be("fees");
                producerFeesServiceConfig.ServiceClientId.Should().Be("ServiceClientId");

                var complianceSchemeFeesServiceConfig = optionsMonitor.Get("ComplianceSchemeFeesService");
                complianceSchemeFeesServiceConfig.Should().NotBeNull();
                complianceSchemeFeesServiceConfig.Url.Should().Be("https://compliancescheme.fees.service");
                complianceSchemeFeesServiceConfig.EndPointName.Should().Be("fees");

                // Act and Assert services
                var httpPaymentsService = serviceProvider?.GetService<IHttpOnlinePaymentsService>();
                httpPaymentsService.Should().NotBeNull();
                httpPaymentsService.Should().BeOfType<HttpOnlinePaymentsService>();

                var httpOfflinePaymentsService = serviceProvider?.GetService<IHttpOfflinePaymentsService>();
                httpOfflinePaymentsService.Should().NotBeNull();
                httpOfflinePaymentsService.Should().BeOfType<HttpOfflinePaymentsService>();

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
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ProducerFeesService)}:{nameof(Service.ServiceClientId)}", "ServiceClientId" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.Url)}", null! },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.EndPointName)}", "payment" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" }
            };

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData!)
                .Build();

            // Act
            Action act = () => _services?.AddFacadeDependencies(configurationBuilder).BuildServiceProvider().GetService<IHttpOnlinePaymentsService>();

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("PaymentService Url configuration is missing.");
        }

        [TestMethod]
        public void AddFacadeDependencies_WithMissingEndPointNameConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var configurationData = new Dictionary<string, string>
            {
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ProducerFeesService)}:{nameof(Service.ServiceClientId)}", "ServiceClientId" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.Url)}", "https://payment.service" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.EndPointName)}", null! },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" }
            };

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData!)
                .Build();

            // Act
            Action act = () => _services?.AddFacadeDependencies(configurationBuilder).BuildServiceProvider().GetService<IHttpOnlinePaymentsService>();

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("PaymentService EndPointName configuration is missing.");
        }

        [TestMethod]
        public void AddFacadeOfflinePaymentDependencies_WithMissingUrlConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var configurationData = new Dictionary<string, string>
            {
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ProducerFeesService)}:{nameof(Service.ServiceClientId)}", "ServiceClientId" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.Url)}", "https://payment.service" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.EndPointName)}", "payment" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.OfflinePaymentService)}:{nameof(Service.Url)}", null! },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.OfflinePaymentService)}:{nameof(Service.EndPointName)}", "offline-payment" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.OfflinePaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" }
            };

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData!)
                .Build();

            // Act
            Action act = () => _services?.AddFacadeDependencies(configurationBuilder).BuildServiceProvider().GetService<IHttpOfflinePaymentsService>();

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("OfflinePaymentService Url configuration is missing.");
        }

        [TestMethod]
        public void AddFacadeOfflinePaymentsDependencies_WithMissingEndPointNameConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var configurationData = new Dictionary<string, string>
            {
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.ProducerFeesService)}:{nameof(Service.ServiceClientId)}", "ServiceClientId" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.Url)}", "https://payment.service" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.EndPointName)}", "payment" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.PaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.OfflinePaymentService)}:{nameof(Service.Url)}", "https://offline-payment.service" },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.OfflinePaymentService)}:{nameof(Service.EndPointName)}", null! },
                { $"{ServicesConfiguration.SectionName}:{nameof(ServicesConfiguration.OfflinePaymentService)}:{nameof(Service.HttpClientName)}", "HttpClient" }
            };

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData!)
                .Build();

            // Act
            Action act = () => _services?.AddFacadeDependencies(configurationBuilder).BuildServiceProvider().GetService<IHttpOfflinePaymentsService>();

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("OfflinePaymentService EndPointName configuration is missing.");
        }
    }
}