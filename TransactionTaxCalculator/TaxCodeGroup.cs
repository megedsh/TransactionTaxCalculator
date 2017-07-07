namespace TransactionTaxCalculator
{
    public class TaxCodeGroup
    {
        public string TaxCode;
        public decimal TaxRate;
        public decimal TotalWithTax;
        public decimal TotalTax;
        public decimal TotalWithoutTax;
        public override string ToString()
        {
            return string.Format("Code={4}, Rate={0}, Before={1}, Tax={2}, After={3}", TaxRate, TotalWithoutTax, TotalTax, TotalWithTax, TaxCode);
        }
    }
}