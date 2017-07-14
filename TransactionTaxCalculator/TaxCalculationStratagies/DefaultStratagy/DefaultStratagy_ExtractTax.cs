using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionTaxCalculator.TaxCalculationStratagies.DefaultStratagy
{
    public class DefaultStratagy_ExtractTax : BaseDefaultStratagy
    {
        public static DefaultStratagy_ExtractTax Instance = new DefaultStratagy_ExtractTax();

        public override TaxMethods TaxMethod
        {
            get { return TaxMethods.ExtractTax; }
        }
    }
}
