using System;
using System.Collections.Generic;
using System.Linq;

namespace TransactionTaxCalculator
{
    public class TransactionTaxCalculator
    {
        private TaxMethods _taxMethod;

        public CalculateTransactionResult Calculate(TransactionCalculatorArgs args)
        {
            try
            {
                ValidateArguments(args);
                var helper = new CalculateTaxHelper(args.Lines, args.GlobalDiscountAmount, args.GlobalDiscountPct, args.TaxMethod);
                _taxMethod = args.TaxMethod;
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

            if (helper.DiscountAmount == 0)
            {
                firstCalc.TotalTransactionBeforeDiscountWithoutTax = firstCalc.TotalTransactionAfterDiscountWithTax - firstCalc.TotalTax;
                firstCalc.TotalDiscountWithoutTax = firstCalc.TotalTransactionBeforeDiscountWithoutTax - firstCalc.TotalTransactionAfterDiscountWithoutTax;
                firstCalc.TotalTaxWithoutDiscount = firstCalc.TotalTax;
                firstCalc.TaxRateGroups.TaxGroupedByTaxRateBeforeDiscount = firstCalc.TaxRateGroups.TaxGroupedByTaxRate;
                firstCalc.TaxCodeGroups.TaxGroupedByTaxCodeBeforeDiscount = firstCalc.TaxCodeGroups.TaxGroupedByTaxCode;
                return firstCalc;
            }

            // Calculate again but without the discount
            var helper2 = new CalculateTaxHelper(helper.TransLines, 0, 0, helper.TaxMethod);
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
            CreateTaxGroupTable(helper);
            CalcTaxForEachGroup(helper);

            if (helper.PositiveValuesExist)
            {
                distributeRemainders(true, helper);
            }
            if (helper.NegativeValuesExist)
            {
                distributeRemainders(false, helper);
            }

            groupPositiveAndNegativeRatesGroups(helper);
            groupPositiveAndNegativeCodeGroups(helper);

            return CreateResult(helper);
        }

        private CalculateTransactionResult CreateResult(CalculateTaxHelper helper)
        {

            CalculateTransactionResult res = new CalculateTransactionResult();

            res.TotalTransactionItems = helper.BrutoTotals.TotalItems;
            res.LastTransactionLine = helper.BrutoTotals.LastLine;
            res.TaxCodeGroups.PositiveTaxGroupedByTaxCode = helper.PositiveTaxGroupedByTaxCode.Select(pair => pair.Value);
            res.TaxCodeGroups.NegativeTaxGroupedByTaxCode = helper.NegativeTaxGroupedByTaxCode.Select(pair => pair.Value);
            res.TaxRateGroups.PositiveTaxGroupedByTaxRate = helper.PositiveTaxGroupedByTaxRate.Select(pair => pair.Value);
            res.TaxRateGroups.NegativeTaxGroupedByTaxRate = helper.NegativeTaxGroupedByTaxRate.Select(pair => pair.Value);
            res.TaxRateGroups.TaxGroupedByTaxRate = helper.TaxGroupedByTaxRate.Select(pair => pair.Value).OrderBy(trg => trg.TaxRate);
            res.TaxCodeGroups.TaxGroupedByTaxCode = helper.TaxGroupedByTaxCode.Select(pair => pair.Value).OrderBy(tcg => tcg.TaxCode);
            res.TotalPositiveTax = helper.PositiveTaxGroupedByTaxRate.Values.Sum(x => x.TotalTax);
            res.TotalNegativeTax = helper.NegativeTaxGroupedByTaxRate.Values.Sum(x => x.TotalTax);
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
            foreach (var rateGroup in helper.PositiveTaxGroupedByTaxRate.Values)
            {
                addToRateGroup(helper.TaxGroupedByTaxRate, rateGroup, 1);
            }

            foreach (var rateGroup in helper.NegativeTaxGroupedByTaxRate.Values)
            {
                addToRateGroup(helper.TaxGroupedByTaxRate, rateGroup, -1);
            }
        }

        private static void groupPositiveAndNegativeCodeGroups(CalculateTaxHelper helper)
        {
            foreach (var codeGroup in helper.PositiveTaxGroupedByTaxCode.Values)
            {
                addToCodeGroup(helper.TaxGroupedByTaxCode, codeGroup, 1);
            }

            foreach (var codeGroup in helper.NegativeTaxGroupedByTaxCode.Values)
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

        private void CreateTaxGroupTable(CalculateTaxHelper helper)
        {
            foreach (var line in helper.TransLines)
            {
                string groupCode = TaxGroupForCalculations.GroupId(PosOrNeg(line.LineTotal), line.TaxCode, line.TaxRate);
                TaxGroupForCalculations c;
                if (false == helper.TaxGroupsForCalculations.TryGetValue(groupCode, out c))
                {
                    helper.TaxGroupsForCalculations[groupCode] = c = new TaxGroupForCalculations(PosOrNeg(line.LineTotal), line.TaxCode, line.TaxRate);
                }

                c.TotalForGroupBeforeDiscount += line.LineTotal;
                if (c.PositiveOrNegative == -1)
                {
                    helper.NegativeValuesExist = true;
                }
                else
                {
                    helper.PositiveValuesExist = true;
                }
            }
        }

        private void CalcTaxForEachGroup(CalculateTaxHelper helper)
        {
            foreach (var group in helper.TaxGroupsForCalculations.Values)
            {
                group.DiscountAmountForGroup = GetDiscountAmount(helper, group.TotalForGroupBeforeDiscount);
                group.TruncatedDiscountAmountForGroup = Truncate(group.DiscountAmountForGroup);
                group.TotalForGroupAfterDiscount = group.TotalForGroupBeforeDiscount - group.DiscountAmountForGroup;
                group.TotalForGroupAfterDiscountRounded = Math.Round(group.TotalForGroupAfterDiscount, 2);
                group.Remainder = group.DiscountAmountForGroup - group.TruncatedDiscountAmountForGroup;
                if (group.TotalForGroupBeforeDiscount >= 0)
                {
                    helper.RemainderSumPositive += group.Remainder;
                }
                else
                {
                    helper.RemainderSumNegative += group.Remainder;
                }
            }
        }

        private decimal GetDiscountAmount(CalculateTaxHelper helper, decimal bruto)
        {
            decimal discountPct = GetRealDiscountPct(helper);
            return Math.Round(bruto * discountPct, 8);
        }

        private decimal GetRealDiscountPct(CalculateTaxHelper helper)
        {
            switch (_taxMethod)
            {
                case TaxMethods.AddTax:
                    if (helper.BrutoTotals.TotalAmountWithoutTax != 0)
                    {
                        return helper.DiscountAmount / helper.BrutoTotals.TotalAmountWithoutTax;
                    }
                    return 0;
                default:
                    if (helper.BrutoTotals.TotalAmountWithTax != 0)
                    {
                        return helper.DiscountAmount / helper.BrutoTotals.TotalAmountWithTax;
                    }
                    return 0;
            }
        }

        private decimal Truncate(decimal p)
        {
            return Math.Truncate(p * 100) / 100;
        }

        private void distributeRemainders(bool posOrNeg, CalculateTaxHelper helper)
        {
            int sign = posOrNeg ? 1 : -1;
            var vatGroups = helper.TaxGroupsForCalculations.Select(vg => vg.Value).Where(x => x.PositiveOrNegative == sign);
            var sortedList = vatGroups.OrderByDescending(o => o.Remainder).ThenByDescending(o => o.TotalForGroupBeforeDiscount);
            int groupCounter = 0;
            decimal howManyDistributions = Math.Abs(posOrNeg ? helper.RemainderSumPositive : helper.RemainderSumNegative) / 0.01m;

            foreach (var group in sortedList)
            {
                groupCounter++;
                group.TotalForGroupBeforeDiscount = Math.Abs(group.TotalForGroupBeforeDiscount);
                group.TruncatedDiscountAmountForGroup = Math.Abs(group.TruncatedDiscountAmountForGroup);

                if (groupCounter <= howManyDistributions)
                {
                    group.DistributedRemainder = 0.01m;
                }
                group.DiscountAmountForGroupWithRemainder = group.TruncatedDiscountAmountForGroup + group.DistributedRemainder;
                group.TotalForGroupAfterDiscountAndRemainder = group.TotalForGroupBeforeDiscount - group.DiscountAmountForGroupWithRemainder;

                group.TotalVATAfterDiscount =  Utils.GetSaleTaxDue(group.TotalForGroupAfterDiscountAndRemainder, group.TaxRate, helper.TaxMethod);
                group.TotalVATBeforeDiscount = Utils.GetSaleTaxDue(group.TotalForGroupBeforeDiscount, group.TaxRate, helper.TaxMethod);
                
                addToTaxRateGroups(posOrNeg, group.TaxRate, group.TotalForGroupAfterDiscountAndRemainder, helper);
                addToTaxCodeGroups(posOrNeg, group.TaxCode, group.TaxRate, group.TotalForGroupAfterDiscountAndRemainder, helper);
            }
        }

        private void addToTaxRateGroups(bool posOrNeg, decimal taxRate, decimal amount, CalculateTaxHelper helper)
        {
            TaxRateGroup trg;
            Dictionary<decimal, TaxRateGroup> groupsDict = resolvRateList(posOrNeg, helper);
            if (false == groupsDict.TryGetValue(taxRate, out trg))
            {
                groupsDict[taxRate] = trg = new TaxRateGroup(taxRate);
            }

            if (_taxMethod == TaxMethods.AddTax)
            {
                trg.TotalWithoutTax += amount;
                trg.TotalTax = Utils.GetSaleTaxDue(trg.TotalWithoutTax, trg.TaxRate, _taxMethod);
                trg.TotalWithTax = trg.TotalWithoutTax + trg.TotalTax;
            }
            else
            {
                trg.TotalWithTax += amount;
                trg.TotalTax = Utils.GetSaleTaxDue(trg.TotalWithTax, trg.TaxRate, _taxMethod);
                trg.TotalWithoutTax = trg.TotalWithTax - trg.TotalTax;
            }
        }

        private void addToTaxCodeGroups(bool posOrNeg, string taxCode, decimal taxRate, decimal amount, CalculateTaxHelper helper)
        {
            TaxCodeGroup taxCodeGroup;
            Dictionary<string, TaxCodeGroup> groupsDict = resolvGroupList(posOrNeg, helper);
            if (false == groupsDict.TryGetValue(taxCode, out taxCodeGroup))
            {
                groupsDict[taxCode] = taxCodeGroup = new TaxCodeGroup(taxCode, taxRate);
            }

            if (_taxMethod == TaxMethods.AddTax)
            {
                taxCodeGroup.TotalWithoutTax += amount;
                taxCodeGroup.TotalTax = Utils.GetSaleTaxDue(taxCodeGroup.TotalWithoutTax, taxCodeGroup.TaxRate, _taxMethod);
                taxCodeGroup.TotalWithTax = taxCodeGroup.TotalWithoutTax + taxCodeGroup.TotalTax;
            }
            else
            {
                taxCodeGroup.TotalWithTax += amount;
                taxCodeGroup.TotalTax = Utils.GetSaleTaxDue(taxCodeGroup.TotalWithTax, taxCodeGroup.TaxRate, _taxMethod);
                taxCodeGroup.TotalWithoutTax = taxCodeGroup.TotalWithTax - taxCodeGroup.TotalTax;
            }

        }

        private static Dictionary<decimal, TaxRateGroup> resolvRateList(bool posOrNeg, CalculateTaxHelper helper)
        {
            if (posOrNeg)
            {
                return helper.PositiveTaxGroupedByTaxRate;
            }
            return helper.NegativeTaxGroupedByTaxRate;
        }

        private static Dictionary<string, TaxCodeGroup> resolvGroupList(bool posOrNeg, CalculateTaxHelper helper)
        {

            if (posOrNeg)
            {
                return helper.PositiveTaxGroupedByTaxCode;
            }
            return helper.NegativeTaxGroupedByTaxCode;
        }

        private short PosOrNeg(decimal value)
        {
            if (value >= 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
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