using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Facade.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ServicesConfiguration
    {
        public static string SectionName => "Services";

        public Service PaymentServiceAPI { get; set; } = new Service();
    }

    public class Service
    {
        public string? Url { get; set; }
    }
}
