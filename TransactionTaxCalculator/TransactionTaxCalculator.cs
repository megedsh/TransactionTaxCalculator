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
            CalculateTransactionResult res = new CalculateTransactionResult();
            res.TotalTransactionItems = helper.BrutoTotals.TotalItems;
            res.LastTransactionLine = helper.BrutoTotals.LastLine;

            CreateTaxGroupTable(helper);
            CalcTaxForEachGroup(helper);
            if (helper.PositiveValuesExist)
            {
                distributeRemainders(true, res, helper);
            }
            if (helper.NegativeValuesExist)
            {
                distributeRemainders(false, res, helper);
            }
            GroupAndSortTaxes(res);
            updateTransactionSum(res, helper);
            return res;
        }

        private void GroupAndSortTaxes(CalculateTransactionResult res)
        {
            groupPositiveAndNegativeRatesGroups(res);
            groupPositiveAndNegativeCodeGroups(res);

            res.TaxRateGroups.TaxGroupedByTaxRate = res.TaxRateGroups.TaxGroupedByTaxRate.OrderBy(x => x.TaxRate).ToList();
            res.TotalPositiveTax = res.TaxRateGroups.PositiveTaxGroupedByTaxRate.Sum(x => x.TotalTax);
            res.TotalNegativeTax = res.TaxRateGroups.NegativeTaxGroupedByTaxRate.Sum(x => x.TotalTax);
            res.TotalTax = res.TotalPositiveTax - res.TotalNegativeTax;
        }

        private static void groupPositiveAndNegativeRatesGroups(CalculateTransactionResult res)
        {
            if (res.TaxRateGroups.NegativeTaxGroupedByTaxRate.Count == 0 && res.TaxRateGroups.PositiveTaxGroupedByTaxRate.Count > 0)
            {
                foreach (var vat in res.TaxRateGroups.PositiveTaxGroupedByTaxRate)
                {
                    res.TaxRateGroups.TaxGroupedByTaxRate.Add(cloneGroup(vat, 1));
                }
            }
            if (res.TaxRateGroups.PositiveTaxGroupedByTaxRate.Count == 0 && res.TaxRateGroups.NegativeTaxGroupedByTaxRate.Count > 0)
            {
                foreach (var vat in res.TaxRateGroups.NegativeTaxGroupedByTaxRate)
                {
                    res.TaxRateGroups.TaxGroupedByTaxRate.Add(cloneGroup(vat, -1));
                }
            }
            if (res.TaxRateGroups.PositiveTaxGroupedByTaxRate.Count > 0 && res.TaxRateGroups.NegativeTaxGroupedByTaxRate.Count > 0)
            {
                foreach (var vat in res.TaxRateGroups.PositiveTaxGroupedByTaxRate)
                {
                    res.TaxRateGroups.TaxGroupedByTaxRate.Add(cloneGroup(vat, 1));
                }
                foreach (var vat in res.TaxRateGroups.NegativeTaxGroupedByTaxRate)
                {
                    TaxRateGroup c = res.TaxRateGroups.TaxGroupedByTaxRate.Find(x => x.TaxRate == vat.TaxRate);

                    if (c != null)
                    {
                        c.TotalWithTax -= vat.TotalWithTax;
                        c.TotalTax -= vat.TotalTax;
                        c.TotalWithoutTax -= vat.TotalWithoutTax;
                    }
                    else
                    {
                        res.TaxRateGroups.TaxGroupedByTaxRate.Add(cloneGroup(vat, -1));
                    }
                }
            }
        }

        private static void groupPositiveAndNegativeCodeGroups(CalculateTransactionResult res)
        {
            if (res.TaxCodeGroups.NegativeTaxGroupedByTaxCode.Count == 0 && res.TaxCodeGroups.PositiveTaxGroupedByTaxCode.Count > 0)
            {
                foreach (var vat in res.TaxCodeGroups.PositiveTaxGroupedByTaxCode)
                {
                    res.TaxCodeGroups.TaxGroupedByTaxCode.Add(cloneGroup(vat, 1));
                }
            }
            if (res.TaxCodeGroups.PositiveTaxGroupedByTaxCode.Count == 0 && res.TaxCodeGroups.NegativeTaxGroupedByTaxCode.Count > 0)
            {
                foreach (var vat in res.TaxCodeGroups.NegativeTaxGroupedByTaxCode)
                {
                    res.TaxCodeGroups.TaxGroupedByTaxCode.Add(cloneGroup(vat, -1));
                }
            }
            if (res.TaxCodeGroups.PositiveTaxGroupedByTaxCode.Count > 0 && res.TaxCodeGroups.NegativeTaxGroupedByTaxCode.Count > 0)
            {
                foreach (var vat in res.TaxCodeGroups.PositiveTaxGroupedByTaxCode)
                {
                    res.TaxCodeGroups.TaxGroupedByTaxCode.Add(cloneGroup(vat, 1));
                }
                foreach (var vat in res.TaxCodeGroups.NegativeTaxGroupedByTaxCode)
                {
                    TaxCodeGroup c = res.TaxCodeGroups.TaxGroupedByTaxCode.Find(x => x.TaxCode == vat.TaxCode);

                    if (c != null)
                    {
                        c.TotalWithTax -= vat.TotalWithTax;
                        c.TotalTax -= vat.TotalTax;
                        c.TotalWithoutTax -= vat.TotalWithoutTax;
                    }
                    else
                    {
                        res.TaxCodeGroups.TaxGroupedByTaxCode.Add(cloneGroup(vat, -1));
                    }
                }
            }
        }

        private static TaxRateGroup cloneGroup(TaxRateGroup vat, int factor)
        {
            return new TaxRateGroup()
            {
                TaxRate = vat.TaxRate,
                TotalWithTax = vat.TotalWithTax * factor,
                TotalTax = vat.TotalTax * factor,
                TotalWithoutTax = vat.TotalWithoutTax * factor
            };
        }

        private static TaxCodeGroup cloneGroup(TaxCodeGroup vat, int factor)
        {
            return new TaxCodeGroup()
            {
                TaxCode = vat.TaxCode,
                TaxRate = vat.TaxRate,
                TotalWithTax = vat.TotalWithTax * factor,
                TotalTax = vat.TotalTax * factor,
                TotalWithoutTax = vat.TotalWithoutTax * factor
            };
        }

        private void updateTransactionSum(CalculateTransactionResult res, CalculateTaxHelper helper)
        {
            if (helper.PositiveValuesExist)
            {
                res.TotalPositiveIncludingTax = res.TaxRateGroups.PositiveTaxGroupedByTaxRate.Sum(x => x.TotalWithTax);
            }
            if (helper.NegativeValuesExist)
            {
                res.TotalNegativeIncludingTax = res.TaxRateGroups.NegativeTaxGroupedByTaxRate.Sum(x => x.TotalWithTax);
            }

            res.SumLinesWithTax = helper.BrutoTotals.TotalAmountWithTax;
            res.SumLinesWithoutTax = helper.BrutoTotals.TotalAmountWithoutTax;

            res.TotalTransactionBeforeDiscountWithTax = res.SumLinesWithTax;
            res.TotalDiscountWithTax = res.SumLinesWithTax - (res.TotalPositiveIncludingTax - res.TotalNegativeIncludingTax);
            res.TotalDiscountWithoutTax = res.TotalTransactionBeforeDiscountWithoutTax - res.TotalTransactionAfterDiscountWithoutTax;
            res.TotalTransactionAfterDiscountWithTax = res.TotalPositiveIncludingTax - res.TotalNegativeIncludingTax;
            res.TotalTax = res.TotalPositiveTax - res.TotalNegativeTax;
            res.TotalTransactionAfterDiscountWithoutTax = res.TaxRateGroups.TaxGroupedByTaxRate.Sum(x => x.TotalWithoutTax);
        }

        private static decimal GetSaleTaxDue(decimal amount, decimal taxRatePct, TaxMethods taxMetod)
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

        private void CreateTaxGroupTable(CalculateTaxHelper helper)
        {
            foreach (var line in helper.TransLines)
            {
                var c = helper.TaxGroupsForCalculations.Find(x =>
                    x.PositiveOrNegative == PosOrNeg(line.LineTotal) &&
                    x.TaxCode == line.TaxCode &&
                    x.TaxPct == line.TaxRate);
                if (c == null)
                {
                    c = new TaxGroupForCalculations()
                    {
                        PositiveOrNegative = PosOrNeg(line.LineTotal),
                        TaxCode = line.TaxCode,
                        TaxPct = line.TaxRate
                    };
                    helper.TaxGroupsForCalculations.Add(c);
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

            foreach (var group in helper.TaxGroupsForCalculations)
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

        private void distributeRemainders(bool posOrNeg, CalculateTransactionResult res, CalculateTaxHelper helper)
        {
            int sign = posOrNeg ? 1 : -1;
            var vatGroups = helper.TaxGroupsForCalculations.Where(x => x.PositiveOrNegative == sign);
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

                group.TotalVATAfterDiscount = GetSaleTaxDue(group.TotalForGroupAfterDiscountAndRemainder, group.TaxPct, helper.TaxMethod);
                group.TotalVATBeforeDiscount = GetSaleTaxDue(group.TotalForGroupBeforeDiscount, group.TaxPct, helper.TaxMethod);


                addToTaxRateGroups(posOrNeg, group.TaxPct, group.TotalForGroupAfterDiscountAndRemainder, res);
                addToTaxCodeGroups(posOrNeg, group.TaxCode, group.TaxPct, group.TotalForGroupAfterDiscountAndRemainder, res);
            }
        }

        private void addToTaxRateGroups(bool posOrNeg, decimal taxRate, decimal amount, CalculateTransactionResult res)
        {
            List<TaxRateGroup> groupsList;
            if (posOrNeg)
            {
                groupsList = res.TaxRateGroups.PositiveTaxGroupedByTaxRate;
            }
            else
            {
                groupsList = res.TaxRateGroups.NegativeTaxGroupedByTaxRate;
            }

            addToTaxRateGroupList(taxRate, amount, groupsList);
        }

        private void addToTaxRateGroupList(decimal taxRate, decimal amount, List<TaxRateGroup> groupsList)
        {
            var c = groupsList.Find(x => x.TaxRate == taxRate);

            if (c == null)
            {
                c = new TaxRateGroup() { TaxRate = taxRate };
                groupsList.Add(c);
            }

            if (_taxMethod == TaxMethods.AddTax)
            {
                c.TotalWithoutTax += amount;
                c.TotalTax = GetSaleTaxDue(c.TotalWithoutTax, c.TaxRate, _taxMethod);
                c.TotalWithTax = c.TotalWithoutTax + c.TotalTax;
            }
            else
            {
                c.TotalWithTax += amount;
                c.TotalTax = GetSaleTaxDue(c.TotalWithTax, c.TaxRate, _taxMethod);
                c.TotalWithoutTax = c.TotalWithTax - c.TotalTax;
            }
        }

        private void addToTaxCodeGroups(bool posOrNeg, string taxCode, decimal taxRate, decimal amount, CalculateTransactionResult res)
        {
            List<TaxCodeGroup> groupsList;
            if (posOrNeg)
            {
                groupsList = res.TaxCodeGroups.PositiveTaxGroupedByTaxCode;
            }
            else
            {
                groupsList = res.TaxCodeGroups.NegativeTaxGroupedByTaxCode;
            }

            addToTaxCodeGroupList(taxCode, taxRate, amount, groupsList);
        }

        private void addToTaxCodeGroupList(string taxCode, decimal taxRate, decimal amount, List<TaxCodeGroup> groupsList)
        {
            var c = groupsList.Find(x => x.TaxCode == taxCode);

            if (c == null)
            {
                c = new TaxCodeGroup() { TaxCode = taxCode, TaxRate = taxRate };
                groupsList.Add(c);
            }

            if (_taxMethod == TaxMethods.AddTax)
            {
                c.TotalWithoutTax += amount;
                c.TotalTax = GetSaleTaxDue(c.TotalWithoutTax, c.TaxRate, _taxMethod);
                c.TotalWithTax = c.TotalWithoutTax + c.TotalTax;
            }
            else
            {
                c.TotalWithTax += amount;
                c.TotalTax = GetSaleTaxDue(c.TotalWithTax, c.TaxRate, _taxMethod);
                c.TotalWithoutTax = c.TotalWithTax - c.TotalTax;
            }
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


        private class CalculateTaxHelper
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
            public List<TaxGroupForCalculations> TaxGroupsForCalculations = new List<TaxGroupForCalculations>();

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
                    var taxDueForLine = GetSaleTaxDue(line.LineTotal, line.TaxRate, TaxMethod);

                    res.TotalItems += line.Qty;
                    if (TaxMethod == TaxMethods.AddTax)
                    {
                        res.TotalAmountWithoutTax += line.LineTotal;
                        res.TotalAmountWithTax += (line.LineTotal + taxDueForLine);
                    }
                    else
                    {
                        res.TotalAmountWithTax += line.LineTotal;
                    }

                    res.TotalLines++;
                    res.LastLine = line.LineID > res.LastLine ? line.LineID : res.LastLine;

                    var c = res.TaxGroupsCalculatedFromItems.Find(x => x.TaxRate == line.TaxRate);
                    if (c != null)
                    {
                        c.TotalTax += taxDueForLine;
                    }
                    else
                    {
                        res.TaxGroupsCalculatedFromItems.Add(new TaxRateGroup()
                        {
                            TaxRate = line.TaxRate,
                            TotalTax = taxDueForLine
                        });
                    }
                }
                return res;
            }
        }
    }

    public enum TaxMethods { NotSet = 0, AddTax = 1, ExtractTax = 2, NoTax = 3 };
}