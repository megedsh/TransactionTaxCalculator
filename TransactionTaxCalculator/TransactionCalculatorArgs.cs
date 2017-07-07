using System.Collections.Generic;

namespace TransactionTaxCalculator
{
    public class TransactionCalculatorArgs
    {
        public IEnumerable<ITransactionLine> Lines;
        public decimal GlobalDiscountPct;
        public decimal GlobalDiscountAmount;
        public TaxMethods TaxMethod = TaxMethods.ExtractTax;
    }
}