using System;

namespace TransactionTaxCalculator
{
    public class CalculateTransactionResult
    {
        public bool Success;

        public decimal TotalPositiveIncludingTax;
        public decimal TotalNegativeIncludingTax;

        public decimal TotalTax;
        public decimal TotalTaxWithoutDiscount;
        public decimal TotalPositiveTax;
        public decimal TotalNegativeTax;

        public decimal TotalDiscountWithTax;
        public decimal TotalDiscountWithoutTax;

        public decimal SumLinesWithTax;
        public decimal SumLinesWithoutTax;

        public decimal TotalTransactionBeforeDiscountWithoutTax;
        public decimal TotalTransactionAfterDiscountWithoutTax;
        public decimal TotalTransactionBeforeDiscountWithTax;
        public decimal TotalTransactionAfterDiscountWithTax;

        public decimal LastTransactionLine;
        public decimal TotalTransactionItems;

        public TaxRateGroups TaxRateGroups = new TaxRateGroups();
        public TaxCodeGroups TaxCodeGroups = new TaxCodeGroups();
        
        public Exception Exception;
    }
}