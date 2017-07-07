using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionTaxCalculator
{
    internal class CalculateTaxHelper
    {
        public CalculateTaxHelper(IEnumerable<ITransactionLine> transLines, decimal discountAmount, decimal discountPct, TaxMethods taxMethod)
        {
            TransLines = transLines;
            DiscountAmount = discountAmount;
            DiscountPCT = discountPct;
            TaxMethod = taxMethod;
            BrutoTotals = calculateBruto();
            setDiscountAmount();
        }

        private void setDiscountAmount()
        {
            if (DiscountAmount == 0 && DiscountPCT != 0)
            {
                decimal bruto = TaxMethod == TaxMethods.AddTax
                    ? BrutoTotals.TotalAmountWithoutTax
                    : BrutoTotals.TotalAmountWithTax;
                DiscountAmount = Math.Round((DiscountPCT / 100) * bruto, 2);
            }
        }

        public TransactionBrutoTotals BrutoTotals { get; private set; }
        public TaxMethods TaxMethod { get; }

        public IEnumerable<ITransactionLine> TransLines;
        public Dictionary<string, TaxGroupForCalculations> TaxGroupsForCalculations = new Dictionary<string, TaxGroupForCalculations>();

        public Dictionary<decimal, TaxRateGroup> PositiveTaxGroupedByTaxRate = new Dictionary<decimal, TaxRateGroup>();
        public Dictionary<decimal, TaxRateGroup> NegativeTaxGroupedByTaxRate = new Dictionary<decimal, TaxRateGroup>();

        public Dictionary<string, TaxCodeGroup> PositiveTaxGroupedByTaxCode = new Dictionary<string, TaxCodeGroup>();
        public Dictionary<string, TaxCodeGroup> NegativeTaxGroupedByTaxCode = new Dictionary<string, TaxCodeGroup>();

        public Dictionary<decimal, TaxRateGroup> TaxGroupedByTaxRate = new Dictionary<decimal, TaxRateGroup>();
        public Dictionary<string, TaxCodeGroup> TaxGroupedByTaxCode = new Dictionary<string, TaxCodeGroup>();

        public decimal DiscountAmount;
        public decimal DiscountPCT;
        public decimal RemainderSumPositive;
        public decimal RemainderSumNegative;
        public bool PositiveValuesExist;
        public bool NegativeValuesExist;

        private TransactionBrutoTotals calculateBruto()
        {
            TransactionBrutoTotals res = new TransactionBrutoTotals();
            foreach (var line in TransLines)
            {
                var taxDueForLine = Utils.GetSaleTaxDue(line.LineTotal, line.TaxRate, TaxMethod);

                res.TotalItems += line.Qty;
                if (TaxMethod == TaxMethods.AddTax)
                {
                    res.TotalAmountWithoutTax += line.LineTotal;
                    res.TotalAmountWithTax += (line.LineTotal + taxDueForLine);
                }
                else
                {
                    res.TotalAmountWithoutTax += (line.LineTotal - taxDueForLine);
                    res.TotalAmountWithTax += line.LineTotal;
                }

                res.TotalLines++;
                res.LastLine = line.LineID > res.LastLine ? line.LineID : res.LastLine;

                var c = res.TaxGroupsCalculatedFromItems.Find(x => x.TaxRate == line.TaxRate);
                if (c == null)
                {
                    c = new TaxRateGroup(line.TaxRate);
                    res.TaxGroupsCalculatedFromItems.Add(c);
                }

                c.TotalTax += taxDueForLine;
            }
            return res;
        }
    }
}
