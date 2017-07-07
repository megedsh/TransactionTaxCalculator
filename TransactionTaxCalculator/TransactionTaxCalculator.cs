using System;
using System.Collections.Generic;
using System.Linq;
using TransactionTaxCalculator.TaxCalculationStratagies;
using TransactionTaxCalculator.TaxCalculationStratagies.S1;

namespace TransactionTaxCalculator
{
    public class TransactionTaxCalculator
    {
        private ITaxCalculationStratagy m_calculationStratagy;

        public TransactionTaxCalculator(ITaxCalculationStratagy calculationStratagy)
        {
            m_calculationStratagy = calculationStratagy;
        }

        public CalculateTransactionResult Calculate(TransactionCalculatorArgs args)
        {
            try
            {
                ValidateArguments(args);
                var helper = new CalculateTaxHelper(args.Lines, args.GlobalDiscountAmount, args.GlobalDiscountPct, args.TaxMethod);
                var res = ProcessDocument(helper);
                res.Success = true;
                return res;
            }
            catch (Exception ex)
            {
                return new CalculateTransactionResult() { Success = false, Exception = ex };
            }
        }

        private CalculateTransactionResult ProcessDocument(CalculateTaxHelper helper)
        {
            CalculateTransactionResult firstCalc = calculateResult(helper);

            if (helper.DiscountAmount == 0 && helper.DiscountPCT == 0)
            {
                firstCalc.TotalTransactionBeforeDiscountWithoutTax = firstCalc.TotalTransactionAfterDiscountWithTax - firstCalc.TotalTax;
                firstCalc.TotalDiscountWithoutTax = firstCalc.TotalTransactionBeforeDiscountWithoutTax - firstCalc.TotalTransactionAfterDiscountWithoutTax;
                firstCalc.TotalTaxWithoutDiscount = firstCalc.TotalTax;
                firstCalc.TaxRateGroups.TaxGroupedByTaxRateBeforeDiscount = firstCalc.TaxRateGroups.TaxGroupedByTaxRate;
                firstCalc.TaxCodeGroups.TaxGroupedByTaxCodeBeforeDiscount = firstCalc.TaxCodeGroups.TaxGroupedByTaxCode;
                return firstCalc;
            }

            // Calculate again but without the discount
            var helper2 = new CalculateTaxHelper(helper.TransLines, 0, 0 ,helper.TaxMethod);
            CalculateTransactionResult secondCalc = calculateResult(helper2);
            firstCalc.TotalTransactionBeforeDiscountWithoutTax = secondCalc.TotalTransactionAfterDiscountWithTax - secondCalc.TotalTax;
            firstCalc.TotalTaxWithoutDiscount = secondCalc.TotalTax;
            firstCalc.TaxRateGroups.TaxGroupedByTaxRateBeforeDiscount = secondCalc.TaxRateGroups.TaxGroupedByTaxRate;
            firstCalc.TaxCodeGroups.TaxGroupedByTaxCodeBeforeDiscount = secondCalc.TaxCodeGroups.TaxGroupedByTaxCode;
            firstCalc.TotalDiscountWithoutTax = firstCalc.TotalTransactionBeforeDiscountWithoutTax - firstCalc.TotalTransactionAfterDiscountWithoutTax;
            return firstCalc;
        }

        private CalculateTransactionResult calculateResult(CalculateTaxHelper helper)
        {
            CalculationStratagyResult calcRes = m_calculationStratagy.Calculate(helper);

            helper.PositiveTaxGroupedByTaxRate = calcRes.PositiveTaxGroupedByTaxRate;
            helper.NegativeTaxGroupedByTaxRate = calcRes.NegativeTaxGroupedByTaxRate;
            helper.PositiveTaxGroupedByTaxCode = calcRes.PositiveTaxGroupedByTaxCode;
            helper.NegativeTaxGroupedByTaxCode = calcRes.NegativeTaxGroupedByTaxCode;

            groupPositiveAndNegativeRatesGroups(helper);
            groupPositiveAndNegativeCodeGroups(helper);

            return CreateResult(helper);
        }

        private CalculateTransactionResult CreateResult(CalculateTaxHelper helper)
        {

            CalculateTransactionResult res = new CalculateTransactionResult();

            res.TotalTransactionItems = helper.BrutoTotals.TotalItems;
            res.LastTransactionLine = helper.BrutoTotals.LastLine;
            res.TaxCodeGroups.PositiveTaxGroupedByTaxCode = helper.PositiveTaxGroupedByTaxCode;
            res.TaxCodeGroups.NegativeTaxGroupedByTaxCode = helper.NegativeTaxGroupedByTaxCode;
            res.TaxRateGroups.PositiveTaxGroupedByTaxRate = helper.PositiveTaxGroupedByTaxRate;
            res.TaxRateGroups.NegativeTaxGroupedByTaxRate = helper.NegativeTaxGroupedByTaxRate; 
            res.TaxRateGroups.TaxGroupedByTaxRate = helper.TaxGroupedByTaxRate.Select(pair => pair.Value).OrderBy(trg => trg.TaxRate);
            res.TaxCodeGroups.TaxGroupedByTaxCode = helper.TaxGroupedByTaxCode.Select(pair => pair.Value).OrderBy(tcg => tcg.TaxCode);
            res.TotalPositiveTax = helper.PositiveTaxGroupedByTaxRate.Sum(x => x.TotalTax);
            res.TotalNegativeTax = helper.NegativeTaxGroupedByTaxRate.Sum(x => x.TotalTax);
            res.TotalTax = res.TotalPositiveTax - res.TotalNegativeTax;
            res.TotalPositiveIncludingTax = res.TaxRateGroups.PositiveTaxGroupedByTaxRate.Sum(x => x.TotalWithTax);
            res.TotalNegativeIncludingTax = res.TaxRateGroups.NegativeTaxGroupedByTaxRate.Sum(x => x.TotalWithTax);
            res.SumLinesWithTax = helper.BrutoTotals.TotalAmountWithTax;
            res.SumLinesWithoutTax = helper.BrutoTotals.TotalAmountWithoutTax;
            res.TotalTransactionBeforeDiscountWithTax = res.SumLinesWithTax;
            res.TotalDiscountWithTax = res.SumLinesWithTax - (res.TotalPositiveIncludingTax - res.TotalNegativeIncludingTax);
            res.TotalDiscountWithoutTax = res.TotalTransactionBeforeDiscountWithoutTax - res.TotalTransactionAfterDiscountWithoutTax;
            res.TotalTransactionAfterDiscountWithTax = res.TotalPositiveIncludingTax - res.TotalNegativeIncludingTax;
            res.TotalTransactionAfterDiscountWithoutTax = helper.TaxGroupedByTaxRate.Values.Sum(x => x.TotalWithoutTax);
            return res;
        }

        private static void groupPositiveAndNegativeRatesGroups(CalculateTaxHelper helper)
        {
            foreach (var rateGroup in helper.PositiveTaxGroupedByTaxRate)
            {
                addToRateGroup(helper.TaxGroupedByTaxRate, rateGroup, 1);
            }

            foreach (var rateGroup in helper.NegativeTaxGroupedByTaxRate)
            {
                addToRateGroup(helper.TaxGroupedByTaxRate, rateGroup, -1);
            }
        }

        private static void groupPositiveAndNegativeCodeGroups(CalculateTaxHelper helper)
        {
            foreach (var codeGroup in helper.PositiveTaxGroupedByTaxCode)
            {
                addToCodeGroup(helper.TaxGroupedByTaxCode, codeGroup, 1);
            }

            foreach (var codeGroup in helper.NegativeTaxGroupedByTaxCode)
            {
                addToCodeGroup(helper.TaxGroupedByTaxCode, codeGroup, -1);
            }
        }

        private static void addToCodeGroup(IDictionary<string, TaxCodeGroup> target, TaxCodeGroup rateGroup, int factor)
        {

            TaxCodeGroup tcg;
            if (false == target.TryGetValue(rateGroup.TaxCode, out tcg))
            {
                target[rateGroup.TaxCode] = tcg = new TaxCodeGroup(rateGroup.TaxCode, rateGroup.TaxRate);
            }

            tcg.TotalWithTax += rateGroup.TotalWithTax * factor;
            tcg.TotalTax += rateGroup.TotalTax * factor;
            tcg.TotalWithoutTax += rateGroup.TotalWithoutTax * factor;

        }

        private static void addToRateGroup(IDictionary<decimal, TaxRateGroup> target, TaxRateGroup codeGroup, int factor)
        {
            TaxRateGroup trg;
            if (false == target.TryGetValue(codeGroup.TaxRate, out trg))
            {
                target[codeGroup.TaxRate] = trg = new TaxRateGroup(codeGroup.TaxRate);
            }

            trg.TotalWithTax += codeGroup.TotalWithTax * factor;
            trg.TotalTax += codeGroup.TotalTax * factor;
            trg.TotalWithoutTax += codeGroup.TotalWithoutTax * factor;
        }

        private void ValidateArguments(TransactionCalculatorArgs args)
        {
            if (args.TaxMethod == TaxMethods.NotSet) { throw new Exception("Tax Method not set properly"); }
            if (args.GlobalDiscountPct != 0 && args.GlobalDiscountAmount != 0) { throw new Exception("Set eather Global discount percent or Global discount amount, Not both"); }

            var invalidGroup = args.Lines.
                GroupBy(line => line.TaxCode).
                Select(group => new
                {
                    Code = group.Key,
                    Count = group.Select(line => line.TaxRate).
                Distinct().Count()
                }).
                FirstOrDefault(tc => tc.Count > 1);

            if (invalidGroup != null)
            {
                throw new Exception(string.Format("Group '{0}' has multiple tax rates", invalidGroup.Code));
            }

        }
    }

    public enum TaxMethods { NotSet = 0, AddTax = 1, ExtractTax = 2, NoTax = 3 };
}