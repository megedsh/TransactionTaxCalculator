namespace TransactionTaxCalculator
{
    public class TaxCodeGroup
    {
        public TaxCodeGroup(string taxCode, decimal taxRate)
        {
            TaxCode = taxCode;
            TaxRate = taxRate;
        }

        public string TaxCode { get; private set; }
        public decimal TaxRate { get; private set; }

        public decimal TotalWithTax;
        public decimal TotalTax;
        public decimal TotalWithoutTax;
        public override string ToString()
        {
            return string.Format("Code={4}, Rate={0}, Before={1}, Tax={2}, After={3}", TaxRate, TotalWithoutTax, TotalTax, TotalWithTax, TaxCode);
        }
    }
}