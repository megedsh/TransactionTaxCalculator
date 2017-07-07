using System.Collections.Generic;

namespace TransactionTaxCalculator
{
    public class TaxCodeGroups
    {
        public IEnumerable<TaxCodeGroup> TaxGroupedByTaxCode { get; set; }
        public IEnumerable<TaxCodeGroup> TaxGroupedByTaxCodeBeforeDiscount { get; set; }
        public IEnumerable<TaxCodeGroup> PositiveTaxGroupedByTaxCode { get; set; }
        public IEnumerable<TaxCodeGroup> NegativeTaxGroupedByTaxCode { get; set; }

    }
}