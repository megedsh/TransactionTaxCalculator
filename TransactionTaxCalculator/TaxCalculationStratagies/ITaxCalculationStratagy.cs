using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionTaxCalculator.TaxCalculationStratagies
{
    public interface ITaxCalculationStratagy
    {
        CalculationStratagyResult Calculate(CalculateTaxHelper commonTaxHelper);
    }
}
