using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Facade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ServicesConfiguration
    {
        public static string SectionName => "Services";

        public Service PaymentService { get; set; } = new Service();

        public Service OnlineV2PaymentService { get; set; } = new Service();


        public Service OfflinePaymentService { get; set; } = new Service();

        public Service OfflinePaymentServiceV2 { get; set; } = new Service();

        public Service GovPayService { get; set; } = new Service();
        
        public Service ProducerFeesService { get; set; } = new Service();
        
        public Service ComplianceSchemeFeesService { get; set; } = new Service();
        
        public Service PaymentServiceHealthCheck { get; set; } = new Service();
        
        public Service ProducerResubmissionFeesService { get; set; } = new Service();
        
        public Service RexExpoRegistrationFeesService { get; set; } = new Service();
        
        public Service RexExpoAccreditationFeesService { get; set; } = new Service();
    }

    public class Service
    {
        public string? Url { get; set; }
        public string? EndPointName { get; set; }
        public string? BearerToken { get; set; }
        public string? HttpClientName { get; set; }
        public int? Retries { get; set; }
        public string? ServiceClientId { get; set; }
    }
}