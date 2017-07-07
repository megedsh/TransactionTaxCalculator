using System.Collections.Generic;

namespace TransactionTaxCalculator
{
    public class TaxCodeGroups
    {
        public List<TaxCodeGroup> TaxGroupedByTaxCode = new List<TaxCodeGroup>();
        public List<TaxCodeGroup> PositiveTaxGroupedByTaxCode = new List<TaxCodeGroup>();
        public List<TaxCodeGroup> NegativeTaxGroupedByTaxCode = new List<TaxCodeGroup>();
        public List<TaxCodeGroup> TaxGroupedByTaxCodeBeforeDiscount = new List<TaxCodeGroup>();
    }
}