namespace TransactionTaxCalculator
{
    internal class TaxGroupForCalculations
    {
        public short PositiveOrNegative;
        public string TaxCode;
        public decimal TaxPct;
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