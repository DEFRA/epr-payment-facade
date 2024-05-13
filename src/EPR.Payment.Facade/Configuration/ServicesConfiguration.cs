namespace EPR.Payment.Facade.Configuration
{
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
