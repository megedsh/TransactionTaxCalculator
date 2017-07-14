using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionTaxCalculator.TaxCalculationStratagies.DefaultStratagy
{
    public class DefaultStratagy_AddTax : BaseDefaultStratagy
    {
        public static DefaultStratagy_AddTax Instance = new DefaultStratagy_AddTax();

        public override TaxMethods TaxMethod
        {
            get { return TaxMethods.AddTax; }
        }
    }
}
