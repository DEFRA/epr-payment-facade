namespace EPR.Payment.Facade.Configuration
{
    public class ServicesConfiguration
    {
        public static string SectionName => "Services";

        public Service PaymentServiceAPI { get; set; }
    }

    public class Service
    {
        public string Url { get; set; }
    }
}
