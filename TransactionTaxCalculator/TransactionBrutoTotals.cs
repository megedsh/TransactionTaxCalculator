using System.Collections.Generic;

namespace TransactionTaxCalculator
{
    public class TransactionBrutoTotals
    {
        public decimal TotalAmountWithTax = 0;
        public decimal TotalAmountWithoutTax = 0;
        public decimal TotalItems = 0;
        public decimal TotalLines = 0;
        public decimal LastLine;
        public List<TaxRateGroup> TaxGroupsCalculatedFromItems = new List<TaxRateGroup>();
    }
}