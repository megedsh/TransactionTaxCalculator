namespace TransactionTaxCalculator.TaxCalculationStratagies.DefaultStratagy
{
    internal class TaxGroupForCalculations
    {
        public static string GroupId(short positiveOrNegative, string taxCode, decimal taxRate)
        {
            return string.Format("{0}-{1}-{2}", positiveOrNegative < 0 ? "Neg" : "Pos", taxCode != null ? taxCode : string.Empty, taxRate);
        }

        public TaxGroupForCalculations(short positiveOrNegative, string taxCode, decimal taxRate)
        {
            PositiveOrNegative = positiveOrNegative;
            TaxCode = taxCode;
            TaxRate = taxRate;
        }
        public short PositiveOrNegative { get; private set; }
        public string TaxCode { get; private set; }
        public decimal TaxRate { get; private set; }

        public decimal TotalForGroupBeforeDiscount;
        public decimal DiscountAmountForGroup;
        public decimal TruncatedDiscountAmountForGroup;
        public decimal TotalForGroupAfterDiscount;
        public decimal TotalForGroupAfterDiscountRounded;
        public decimal Remainder;
        public decimal DistributedRemainder;
        public decimal DiscountAmountForGroupWithRemainder;
        public decimal TotalForGroupAfterDiscountAndRemainder;
        public decimal TotalVATBeforeDiscount;
        public decimal TotalVATAfterDiscount;
    }
}