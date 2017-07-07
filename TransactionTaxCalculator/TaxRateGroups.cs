using System.Collections.Generic;

namespace TransactionTaxCalculator
{
    public class TaxRateGroups
    {
        public Dictionary<decimal ,TaxRateGroup> TaxGroupedByTaxRate = new Dictionary<decimal, TaxRateGroup>();
        public Dictionary<decimal, TaxRateGroup> TaxGroupedByTaxRateBeforeDiscount = new Dictionary<decimal, TaxRateGroup>();

        public List<TaxRateGroup> PositiveTaxGroupedByTaxRate = new List<TaxRateGroup>();
        public List<TaxRateGroup> NegativeTaxGroupedByTaxRate = new List<TaxRateGroup>();
    }
}