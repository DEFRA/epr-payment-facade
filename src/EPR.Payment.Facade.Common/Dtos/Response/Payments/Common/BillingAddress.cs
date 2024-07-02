namespace EPR.Payment.Facade.Common.Dtos.Response.Payments.Common
{
    public class BillingAddress
    {
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? Postcode { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
