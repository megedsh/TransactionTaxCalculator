using System.Collections.Generic;

namespace TransactionTaxCalculator
{
    public class TaxCodeGroups
    {
        public Dictionary<string,TaxCodeGroup> TaxGroupedByTaxCode = new Dictionary<string, TaxCodeGroup>();
        public Dictionary<string, TaxCodeGroup> TaxGroupedByTaxCodeBeforeDiscount = new Dictionary<string, TaxCodeGroup>();

        public List<TaxCodeGroup> PositiveTaxGroupedByTaxCode = new List<TaxCodeGroup>();
        public List<TaxCodeGroup> NegativeTaxGroupedByTaxCode = new List<TaxCodeGroup>();
        
    }
}