using System.Collections.Generic;

namespace TransactionTaxCalculator
{
    public class TaxRateGroups
    {
        public List<TaxRateGroup> TaxGroupedByTaxRate = new List<TaxRateGroup>();
        public List<TaxRateGroup> PositiveTaxGroupedByTaxRate = new List<TaxRateGroup>();
        public List<TaxRateGroup> NegativeTaxGroupedByTaxRate = new List<TaxRateGroup>();
        public List<TaxRateGroup> TaxGroupedByTaxRateBeforeDiscount = new List<TaxRateGroup>();
    }
}