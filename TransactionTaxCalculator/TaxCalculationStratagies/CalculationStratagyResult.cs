using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionTaxCalculator.TaxCalculationStratagies
{
    public class CalculationStratagyResult
    {
        public CalculationStratagyResult(IEnumerable<TaxRateGroup> positiveTaxGroupedByTaxRate,
            IEnumerable<TaxRateGroup> negativeTaxGroupedByTaxRate,
            IEnumerable<TaxCodeGroup> positiveTaxGroupedByTaxCode,
            IEnumerable<TaxCodeGroup> negativeTaxGroupedByTaxCode)
        {
            PositiveTaxGroupedByTaxRate = positiveTaxGroupedByTaxRate;
            NegativeTaxGroupedByTaxRate = negativeTaxGroupedByTaxRate;
            PositiveTaxGroupedByTaxCode = positiveTaxGroupedByTaxCode;
            NegativeTaxGroupedByTaxCode = negativeTaxGroupedByTaxCode;
        }

        public IEnumerable<TaxRateGroup> PositiveTaxGroupedByTaxRate { get; private set; }
        public IEnumerable<TaxRateGroup> NegativeTaxGroupedByTaxRate { get; private set; }
        public IEnumerable<TaxCodeGroup> PositiveTaxGroupedByTaxCode { get; private set; }
        public IEnumerable<TaxCodeGroup> NegativeTaxGroupedByTaxCode { get; private set; }
    }
}
