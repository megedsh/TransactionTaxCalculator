using System.Collections.Generic;

namespace TransactionTaxCalculator
{
    public class TaxRateGroups
    {
        public IEnumerable<TaxRateGroup> TaxGroupedByTaxRate { get; set; }
        public IEnumerable<TaxRateGroup> TaxGroupedByTaxRateBeforeDiscount { get; set; }
        public IEnumerable<TaxRateGroup> PositiveTaxGroupedByTaxRate { get; set; }
        public IEnumerable<TaxRateGroup> NegativeTaxGroupedByTaxRate { get; set; }
    }
}