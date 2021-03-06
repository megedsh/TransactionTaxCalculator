namespace TransactionTaxCalculator
{
    public class TaxRateGroup
    {
        public decimal TaxRate { get; private set; }
        public TaxRateGroup(decimal taxRate)
        {
            TaxRate = taxRate;
        }
        public decimal TotalWithTax;
        public decimal TotalTax;
        public decimal TotalWithoutTax;
        public override string ToString()
        {
            return string.Format("Rate={0}, Before={1}, Tax={2}, After={3}", TaxRate, TotalWithoutTax, TotalTax, TotalWithTax);
        }
    }
}