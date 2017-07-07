using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionTaxCalculator
{
    public class Utils
    {
        public static decimal GetSaleTaxDue(decimal amount, decimal taxRatePct, TaxMethods taxMetod)
        {
            switch (taxMetod)
            {
                case TaxMethods.ExtractTax:
                    return Math.Round(amount - amount / (1 + taxRatePct / 100), 2);
                case TaxMethods.AddTax:
                    return Math.Round(amount * (taxRatePct / 100), 2);
                default:
                    return 0;
            }
        }
    }
}
