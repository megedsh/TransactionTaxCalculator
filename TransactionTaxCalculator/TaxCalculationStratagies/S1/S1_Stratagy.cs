using System;
using System.Collections.Generic;
using System.Linq;

namespace TransactionTaxCalculator.TaxCalculationStratagies.S1
{
    public class S1_Stratagy : ITaxCalculationStratagy
    {
        public static S1_Stratagy Default = new S1_Stratagy();
        public const short POSITIVE_SIGN = 1;
        public const short NEGATIVE_SIGN = -1;

        public CalculationStratagyResult Calculate(CalculateTaxHelper commonTaxHelper)
        {
            RemainderHelper remainderHelper = new RemainderHelper();
            Dictionary<string, TaxGroupForCalculations> taxGroupsForCalculations =
                createTaxGroupTable(commonTaxHelper.TransLines);
            calcTaxForEachGroup(commonTaxHelper, remainderHelper, taxGroupsForCalculations);

            distributeRemainders(POSITIVE_SIGN, commonTaxHelper, remainderHelper, taxGroupsForCalculations.Values);
            distributeRemainders(NEGATIVE_SIGN, commonTaxHelper, remainderHelper, taxGroupsForCalculations.Values);
            var positiveTaxGroupedByTaxRate =
                GetGroupedByTaxRate(taxGroupsForCalculations.Values.Where(x => x.PositiveOrNegative == POSITIVE_SIGN),
                    commonTaxHelper.TaxMethod);
            var negativeTaxGroupedByTaxRate =
                GetGroupedByTaxRate(taxGroupsForCalculations.Values.Where(x => x.PositiveOrNegative == NEGATIVE_SIGN),
                    commonTaxHelper.TaxMethod);
            var positiveTaxGroupedByTaxCode =
                GetGroupedByTaxCode(taxGroupsForCalculations.Values.Where(x => x.PositiveOrNegative == POSITIVE_SIGN),
                    commonTaxHelper.TaxMethod);
            var negativeTaxGroupedByTaxCode =
                GetGroupedByTaxCode(taxGroupsForCalculations.Values.Where(x => x.PositiveOrNegative == NEGATIVE_SIGN),
                    commonTaxHelper.TaxMethod);

            return new CalculationStratagyResult(positiveTaxGroupedByTaxRate, negativeTaxGroupedByTaxRate,
                positiveTaxGroupedByTaxCode, negativeTaxGroupedByTaxCode);
        }

        private IEnumerable<TaxCodeGroup> GetGroupedByTaxCode(
            IEnumerable<TaxGroupForCalculations> taxGroupsForCalculations, TaxMethods taxMethod)
        {
            Dictionary<string, TaxCodeGroup> result = new Dictionary<string, TaxCodeGroup>();
            foreach (var group in taxGroupsForCalculations)
            {
                TaxCodeGroup taxCodeGroup;

                if (false == result.TryGetValue(group.TaxCode, out taxCodeGroup))
                {
                    result[group.TaxCode] = taxCodeGroup = new TaxCodeGroup(group.TaxCode, group.TaxRate);
                }

                if (taxMethod == TaxMethods.AddTax)
                {
                    taxCodeGroup.TotalWithoutTax += group.TotalForGroupAfterDiscountAndRemainder;
                    taxCodeGroup.TotalTax =
                        Utils.GetSaleTaxDue(taxCodeGroup.TotalWithoutTax, taxCodeGroup.TaxRate, taxMethod);
                    taxCodeGroup.TotalWithTax = taxCodeGroup.TotalWithoutTax + taxCodeGroup.TotalTax;
                }
                else
                {
                    taxCodeGroup.TotalWithTax += group.TotalForGroupAfterDiscountAndRemainder;
                    taxCodeGroup.TotalTax =
                        Utils.GetSaleTaxDue(taxCodeGroup.TotalWithTax, taxCodeGroup.TaxRate, taxMethod);
                    taxCodeGroup.TotalWithoutTax = taxCodeGroup.TotalWithTax - taxCodeGroup.TotalTax;
                }
            }
            return result.Values;
        }

        private IEnumerable<TaxRateGroup> GetGroupedByTaxRate(
            IEnumerable<TaxGroupForCalculations> taxGroupsForCalculations, TaxMethods taxMethod)
        {
            Dictionary<decimal, TaxRateGroup> result = new Dictionary<decimal, TaxRateGroup>();
            foreach (TaxGroupForCalculations group in taxGroupsForCalculations)
            {
                TaxRateGroup trg;
                if (false == result.TryGetValue(group.TaxRate, out trg))
                {
                    result[group.TaxRate] = trg = new TaxRateGroup(group.TaxRate);
                }

                if (taxMethod == TaxMethods.AddTax)
                {
                    trg.TotalWithoutTax += group.TotalForGroupAfterDiscountAndRemainder;
                    trg.TotalTax = Utils.GetSaleTaxDue(trg.TotalWithoutTax, trg.TaxRate, taxMethod);
                    trg.TotalWithTax = trg.TotalWithoutTax + trg.TotalTax;
                }
                else
                {
                    trg.TotalWithTax += group.TotalForGroupAfterDiscountAndRemainder;
                    trg.TotalTax = Utils.GetSaleTaxDue(trg.TotalWithTax, trg.TaxRate, taxMethod);
                    trg.TotalWithoutTax = trg.TotalWithTax - trg.TotalTax;
                }
            }
            return result.Values;
        }

        private Dictionary<string, TaxGroupForCalculations> createTaxGroupTable(IEnumerable<ITransactionLine> transLines)
        {
            Dictionary<string, TaxGroupForCalculations> result = new Dictionary<string, TaxGroupForCalculations>();
            foreach (var line in transLines)
            {
                string groupCode = TaxGroupForCalculations.GroupId(getSign(line.LineTotal), line.TaxCode, line.TaxRate);
                TaxGroupForCalculations c;
                if (false == result.TryGetValue(groupCode, out c))
                {
                    result[groupCode] = c =
                        new TaxGroupForCalculations(getSign(line.LineTotal), line.TaxCode, line.TaxRate);
                }
                c.TotalForGroupBeforeDiscount += line.LineTotal;
            }

            return result;
        }

        private void calcTaxForEachGroup(CalculateTaxHelper helper, RemainderHelper remainderHelper,
            Dictionary<string, TaxGroupForCalculations> taxGroupsForCalculations)
        {
            foreach (var group in taxGroupsForCalculations.Values)
            {
                group.DiscountAmountForGroup = getDiscountAmount(helper, group.TotalForGroupBeforeDiscount);
                group.TruncatedDiscountAmountForGroup = truncate(group.DiscountAmountForGroup);
                group.TotalForGroupAfterDiscount = group.TotalForGroupBeforeDiscount - group.DiscountAmountForGroup;
                group.TotalForGroupAfterDiscountRounded = Math.Round(group.TotalForGroupAfterDiscount, 2);
                group.Remainder = group.DiscountAmountForGroup - group.TruncatedDiscountAmountForGroup;
                if (group.TotalForGroupBeforeDiscount >= 0)
                {
                    remainderHelper.RemainderSumPositive += group.Remainder;
                }
                else
                {
                    remainderHelper.RemainderSumNegative += group.Remainder;
                }
            }
        }

        private decimal getDiscountAmount(CalculateTaxHelper helper, decimal bruto)
        {
            decimal discountPct = getRealDiscountPct(helper);
            return Math.Round(bruto * discountPct, 8);
        }

        private decimal getRealDiscountPct(CalculateTaxHelper helper)
        {
            decimal discountAmmount = helper.DiscountAmount;

            if (helper.DiscountAmount == 0 && helper.DiscountPCT != 0)
            {
                decimal bruto = helper.TaxMethod == TaxMethods.AddTax
                    ? helper.BrutoTotals.TotalAmountWithoutTax
                    : helper.BrutoTotals.TotalAmountWithTax;
                discountAmmount = Math.Round((discountAmmount / 100) * bruto, 2);
            }

            switch (helper.TaxMethod)
            {
                case TaxMethods.AddTax:
                    if (helper.BrutoTotals.TotalAmountWithoutTax != 0)
                    {
                        return discountAmmount / helper.BrutoTotals.TotalAmountWithoutTax;
                    }
                    return 0;
                default:
                    if (helper.BrutoTotals.TotalAmountWithTax != 0)
                    {
                        return discountAmmount / helper.BrutoTotals.TotalAmountWithTax;
                    }
                    return 0;
            }
        }

        private static decimal truncate(decimal p)
        {
            return Math.Truncate(p * 100) / 100;
        }

        private void distributeRemainders(short sign, CalculateTaxHelper helper, RemainderHelper remainderHelper,
            IEnumerable<TaxGroupForCalculations> taxGroupsForCalculations)
        {
            var vatGroups = taxGroupsForCalculations.Where(x => x.PositiveOrNegative == sign);
            var sortedList = vatGroups.OrderByDescending(o => o.Remainder)
                .ThenByDescending(o => o.TotalForGroupBeforeDiscount);
            int groupCounter = 0;
            decimal howManyDistributions = Math.Abs(sign == POSITIVE_SIGN
                                               ? remainderHelper.RemainderSumPositive
                                               : remainderHelper.RemainderSumNegative) / 0.01m;

            foreach (TaxGroupForCalculations group in sortedList)
            {
                groupCounter++;
                group.TotalForGroupBeforeDiscount = Math.Abs(group.TotalForGroupBeforeDiscount);
                group.TruncatedDiscountAmountForGroup = Math.Abs(group.TruncatedDiscountAmountForGroup);

                if (groupCounter <= howManyDistributions)
                {
                    group.DistributedRemainder = 0.01m;
                }
                group.DiscountAmountForGroupWithRemainder =
                    group.TruncatedDiscountAmountForGroup + group.DistributedRemainder;
                group.TotalForGroupAfterDiscountAndRemainder =
                    group.TotalForGroupBeforeDiscount - group.DiscountAmountForGroupWithRemainder;

                group.TotalVATAfterDiscount = Utils.GetSaleTaxDue(group.TotalForGroupAfterDiscountAndRemainder,
                    group.TaxRate, helper.TaxMethod);
                group.TotalVATBeforeDiscount =
                    Utils.GetSaleTaxDue(group.TotalForGroupBeforeDiscount, group.TaxRate, helper.TaxMethod);
            }
        }

        private short getSign(decimal value)
        {
            if (value >= 0)
            {
                return POSITIVE_SIGN;
            }

            return NEGATIVE_SIGN;
        }
    }
}