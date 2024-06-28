namespace EPR.Payment.Facade.Common.Dtos
{
    public class ProducerSubsidiaryInfo
    {
        public string ProducerType { get; set; } // "L" for Large, "S" for Small
        public int NumberOfSubsidiaries { get; set; }
        public bool PayBaseFee { get; set; } // Indicates if the base fee should be paid
    }
}
