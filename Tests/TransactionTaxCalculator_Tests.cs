using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;


namespace TransactionTaxCalculator.Tests
{
    [TestClass]
    public class CommonTransactionSumCalculator_Tests
    {
        [TestMethod]
        public void SimpleTest1()
        {
            CommonTransactionSumCalculator c = new CommonTransactionSumCalculator();
            TransactionCalculatorArgs args = new TransactionCalculatorArgs();
            args.Lines = new TransactionLine[]
            {
                new TransactionLine(){  TaxRate = 18, TaxCode = "18", LineTotal = 50, Qty = 1}
            };

            const decimal expectedTotalTax = 7.63m;
            const decimal expectedTotalincludingTax = 50m;

            const decimal expectedTotalTax_addTax = 9m;
            const decimal expectedTotalincludingTax_addTaxMethod = 59m;



            args.TaxMethod = TaxMethods.ExtractTax;
            var res = c.Calculate(args);

            Assert.AreEqual(res.TotalPositiveIncludingTax, expectedTotalincludingTax);
            Assert.AreEqual(res.TotalPositiveTax, expectedTotalTax);
            Assert.AreEqual(res.TotalTax, expectedTotalTax);
            Assert.AreEqual(res.TotalTaxWithoutDiscount, expectedTotalTax);
            Assert.AreEqual(res.TotalTransactionAfterDiscountWithoutTax, expectedTotalincludingTax - expectedTotalTax);
            Assert.AreEqual(res.TotalTransactionBeforeDiscountWithTax, expectedTotalincludingTax);
            Assert.AreEqual(res.TotalTransactionAfterDiscountWithTax, expectedTotalincludingTax);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode.Count, 1);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode[0].TotalTax, expectedTotalTax);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.Count, 1);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate[0].TotalTax, expectedTotalTax);
            

            args.TaxMethod = TaxMethods.AddTax;
            var res2 = c.Calculate(args);

            Assert.AreEqual(res2.TotalPositiveIncludingTax, expectedTotalincludingTax_addTaxMethod);
            Assert.AreEqual(res2.TotalPositiveTax, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTax, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTaxWithoutDiscount, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTransactionAfterDiscountWithoutTax, expectedTotalincludingTax_addTaxMethod - expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTransactionBeforeDiscountWithTax, expectedTotalincludingTax_addTaxMethod);
            Assert.AreEqual(res2.TotalTransactionAfterDiscountWithTax, expectedTotalincludingTax_addTaxMethod);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode.Count, 1);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode[0].TotalTax, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate.Count, 1);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate[0].TotalTax, expectedTotalTax_addTax);
        }

        [TestMethod]
        public void TwoTaxCodesOneTaxRateTest()
        {
            CommonTransactionSumCalculator c = new CommonTransactionSumCalculator();
            TransactionCalculatorArgs args = new TransactionCalculatorArgs();
            args.Lines = new TransactionLine[]
            {
                new TransactionLine(){  TaxRate = 18, TaxCode = "A", LineTotal = 25, Qty = 1},
                new TransactionLine(){  TaxRate = 18, TaxCode = "B", LineTotal = 35, Qty = 1}
            };

            const decimal expectedTotalTax = 9.15m;
            const decimal expectedTotalincludingTax = 60m;

            const decimal expectedTotalTax_addTax = 10.8m;
            const decimal expectedTotalincludingTax_addTaxMethod = 70.8m;

            args.TaxMethod = TaxMethods.ExtractTax;
            var res = c.Calculate(args);

            Assert.AreEqual(res.TotalPositiveIncludingTax, expectedTotalincludingTax);
            Assert.AreEqual(res.TotalTax, expectedTotalTax);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode.Count, 2);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode.Sum(x => x.TotalTax), expectedTotalTax);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.Count, 1);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.Sum(x => x.TotalTax), expectedTotalTax);

            args.TaxMethod = TaxMethods.AddTax;
            var res2 = c.Calculate(args);

            Assert.AreEqual(res2.TotalPositiveIncludingTax, expectedTotalincludingTax_addTaxMethod);
            Assert.AreEqual(res2.TotalTax, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode.Count, 2);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode.Sum(x => x.TotalTax), expectedTotalTax_addTax);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate.Count, 1);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate.Sum(x => x.TotalTax), expectedTotalTax_addTax);
        }

        [TestMethod]
        public void TwoTaxCodesTwoTaxRateTest()
        {
            CommonTransactionSumCalculator c = new CommonTransactionSumCalculator();
            TransactionCalculatorArgs args = new TransactionCalculatorArgs();
            args.Lines = new TransactionLine[]
            {
                new TransactionLine(){  TaxRate = 18, TaxCode = "A", LineTotal = 75.5m, Qty = 1},
                new TransactionLine(){  TaxRate = 22, TaxCode = "B", LineTotal = 78.6m, Qty = 1}
            };

            const decimal expectedTotalTax = 25.69m;
            const decimal expectedTotalincludingTax = 154.1m;

            const decimal expectedTotalTax_addTax = 30.88m;
            const decimal expectedTotalincludingTax_addTaxMethod = 184.98m;

            args.TaxMethod = TaxMethods.ExtractTax;
            var res = c.Calculate(args);

            Assert.AreEqual(res.TotalPositiveIncludingTax, expectedTotalincludingTax);
            Assert.AreEqual(res.TotalTax, expectedTotalTax);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode.Count, 2);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode.Sum(x => x.TotalTax), expectedTotalTax);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.Count, 2);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.Sum(x => x.TotalTax), expectedTotalTax);

            args.TaxMethod = TaxMethods.AddTax;
            var res2 = c.Calculate(args);

            Assert.AreEqual(res2.TotalPositiveIncludingTax, expectedTotalincludingTax_addTaxMethod);
            Assert.AreEqual(res2.TotalTax, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode.Count, 2);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode.Sum(x => x.TotalTax), expectedTotalTax_addTax);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate.Count, 2);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate.Sum(x => x.TotalTax), expectedTotalTax_addTax);
        }

        [TestMethod]
        public void NegativeSimple()
        {
            CommonTransactionSumCalculator c = new CommonTransactionSumCalculator();
            TransactionCalculatorArgs args = new TransactionCalculatorArgs();
            args.Lines = new[]
            {
                new TransactionLine(){  TaxRate = 18, TaxCode = "18", LineTotal = -50}
            };

            const decimal expectedTotalTax = -7.63m;
            const decimal expectedTotalincludingTax = -50m;

            const decimal expectedTotalTax_addTax = -9m;
            const decimal expectedTotalincludingTax_addTaxMethod = -59m;



            args.TaxMethod = TaxMethods.ExtractTax;
            var res = c.Calculate(args);

            Assert.AreEqual(res.TotalPositiveIncludingTax, 0);
            Assert.AreEqual(res.TotalPositiveTax, 0);

            Assert.AreEqual(res.TotalNegativeIncludingTax, expectedTotalincludingTax * -1);
            Assert.AreEqual(res.TotalNegativeTax, expectedTotalTax * -1);


            Assert.AreEqual(res.TotalTax, expectedTotalTax);
            Assert.AreEqual(res.TotalTaxWithoutDiscount, expectedTotalTax);
            Assert.AreEqual(res.TotalTransactionAfterDiscountWithoutTax, expectedTotalincludingTax - expectedTotalTax);
            Assert.AreEqual(res.TotalTransactionBeforeDiscountWithTax, expectedTotalincludingTax);
            Assert.AreEqual(res.TotalTransactionAfterDiscountWithTax, expectedTotalincludingTax);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode.Count, 1);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode[0].TotalTax, expectedTotalTax);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.Count, 1);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate[0].TotalTax, expectedTotalTax);

            args.TaxMethod = TaxMethods.AddTax;
            var res2 = c.Calculate(args);
            Assert.AreEqual(res2.TotalPositiveIncludingTax, 0);
            Assert.AreEqual(res2.TotalPositiveTax, 0);
            Assert.AreEqual(res2.TotalNegativeIncludingTax, expectedTotalincludingTax_addTaxMethod * -1);
            Assert.AreEqual(res2.TotalNegativeTax, expectedTotalTax_addTax * -1);
            Assert.AreEqual(res2.TotalTax, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTaxWithoutDiscount, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTransactionAfterDiscountWithoutTax, expectedTotalincludingTax_addTaxMethod - expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTransactionBeforeDiscountWithTax, expectedTotalincludingTax_addTaxMethod);
            Assert.AreEqual(res2.TotalTransactionAfterDiscountWithTax, expectedTotalincludingTax_addTaxMethod);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode.Count, 1);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode[0].TotalTax, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate.Count, 1);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate[0].TotalTax, expectedTotalTax_addTax);
        }

        [TestMethod]
        public void NegativeAndPositive()
        {
            CommonTransactionSumCalculator c = new CommonTransactionSumCalculator();
            TransactionCalculatorArgs args = new TransactionCalculatorArgs();
            args.Lines = new[]
            {
                new TransactionLine(){  TaxRate = 18, TaxCode = "18", LineTotal = -50},
                new TransactionLine(){  TaxRate = 10, TaxCode = "10", LineTotal = 99.99m}
            };

            const decimal expectedTotalTax = 1.46m;
            const decimal expectedTotalincludingTax = 49.99m;

            const decimal expectedTotalTax_addTax = 1m;
            const decimal expectedTotalincludingTax_addTaxMethod = 50.99m;



            args.TaxMethod = TaxMethods.ExtractTax;
            var res = c.Calculate(args);

            Assert.AreEqual(res.TotalPositiveIncludingTax, 99.99m);
            Assert.AreEqual(res.TotalPositiveTax, 9.09m);

            Assert.AreEqual(res.TotalNegativeIncludingTax, 50m);
            Assert.AreEqual(res.TotalNegativeTax, 7.63m);


            Assert.AreEqual(res.TotalTax, expectedTotalTax);
            Assert.AreEqual(res.TotalTaxWithoutDiscount, expectedTotalTax);
            Assert.AreEqual(res.TotalTransactionAfterDiscountWithoutTax, expectedTotalincludingTax - expectedTotalTax);
            Assert.AreEqual(res.TotalTransactionBeforeDiscountWithTax, expectedTotalincludingTax);
            Assert.AreEqual(res.TotalTransactionAfterDiscountWithTax, expectedTotalincludingTax);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode.Count, 2);
            Assert.AreEqual(res.TaxCodeGroups.TaxGroupedByTaxCode.Sum(x => x.TotalTax), expectedTotalTax);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.Count, 2);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.Sum(x => x.TotalTax), expectedTotalTax);

            args.TaxMethod = TaxMethods.AddTax;
            var res2 = c.Calculate(args);
            Assert.AreEqual(res2.TotalPositiveIncludingTax, 109.99m);
            Assert.AreEqual(res2.TotalPositiveTax, 10);
            Assert.AreEqual(res2.TotalNegativeIncludingTax, 59);
            Assert.AreEqual(res2.TotalNegativeTax, 9);
            Assert.AreEqual(res2.TotalTax, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTaxWithoutDiscount, expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTransactionAfterDiscountWithoutTax, expectedTotalincludingTax_addTaxMethod - expectedTotalTax_addTax);
            Assert.AreEqual(res2.TotalTransactionBeforeDiscountWithTax, expectedTotalincludingTax_addTaxMethod);
            Assert.AreEqual(res2.TotalTransactionAfterDiscountWithTax, expectedTotalincludingTax_addTaxMethod);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode.Count, 2);
            Assert.AreEqual(res2.TaxCodeGroups.TaxGroupedByTaxCode.Sum(x => x.TotalTax), expectedTotalTax_addTax);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate.Count, 2);
            Assert.AreEqual(res2.TaxRateGroups.TaxGroupedByTaxRate.Sum(x => x.TotalTax), expectedTotalTax_addTax);
        }

        [TestMethod]
        public void Complex1()
        {
            CommonTransactionSumCalculator c = new CommonTransactionSumCalculator();
            TransactionCalculatorArgs args = new TransactionCalculatorArgs();
            args.Lines = new[]
            {
                new TransactionLine() {TaxRate = 10, TaxCode = "D", LineTotal = 1.5m},
                new TransactionLine() {TaxRate = 10, TaxCode = "I", LineTotal = 2m},
                new TransactionLine() {TaxRate = 10, TaxCode = "D", LineTotal = 2.5m},
                new TransactionLine() {TaxRate = 10, TaxCode = "D", LineTotal = 2.5m},
                new TransactionLine() {TaxRate = 10, TaxCode = "D", LineTotal = 3m},
                new TransactionLine() {TaxRate = 10, TaxCode = "D", LineTotal = 2.5m},
                new TransactionLine() {TaxRate = 10, TaxCode = "D", LineTotal = 1.5m},
                new TransactionLine() {TaxRate = 1, TaxCode = "B", LineTotal = 1m},
                new TransactionLine() {TaxRate = 10, TaxCode = "D", LineTotal = 6m},
                new TransactionLine() {TaxRate = 10, TaxCode = "D", LineTotal = 4m},
                new TransactionLine() {TaxRate = 10, TaxCode = "D", LineTotal = 6m},
                new TransactionLine() {TaxRate = 1, TaxCode = "B", LineTotal = 5m},
                new TransactionLine() {TaxRate = 1, TaxCode = "B", LineTotal = 10m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 158m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 4.5m},
                new TransactionLine() {TaxRate = 20, TaxCode = "J", LineTotal = 87.9m},
                new TransactionLine() {TaxRate = 0, TaxCode = "A", LineTotal = 0.38m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 3.32m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 0.54m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 2.56m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 1.66m},
                new TransactionLine() {TaxRate = 10, TaxCode = "I", LineTotal = 84.39m},
                new TransactionLine() {TaxRate = 10, TaxCode = "I", LineTotal = 34.03m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 109.9m},
                new TransactionLine() {TaxRate = 20, TaxCode = "J", LineTotal = 35m},
                new TransactionLine() {TaxRate = 20, TaxCode = "J", LineTotal = 39.21m},
                new TransactionLine() {TaxRate = 20, TaxCode = "J", LineTotal = 24.99m},
                new TransactionLine() {TaxRate = 20, TaxCode = "J", LineTotal = 16.89m},
                new TransactionLine() {TaxRate = 1, TaxCode = "G", LineTotal = 99.97m},
                new TransactionLine() {TaxRate = 20, TaxCode = "J", LineTotal = 39.39m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 56.99m},
                new TransactionLine() {TaxRate = 10, TaxCode = "I", LineTotal = 32.37m},
                new TransactionLine() {TaxRate = 20, TaxCode = "J", LineTotal = 17.83m},
                new TransactionLine() {TaxRate = 1, TaxCode = "G", LineTotal = 831.98m},
                new TransactionLine() {TaxRate = 1, TaxCode = "G", LineTotal = 284m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 89.9m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 69.99m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 173.21m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 101m},
                new TransactionLine() {TaxRate = 20, TaxCode = "E", LineTotal = 5.5m},
                new TransactionLine() {TaxRate = 20, TaxCode = "E", LineTotal = 24m},
                new TransactionLine() {TaxRate = 25, TaxCode = "F", LineTotal = 6m},
                new TransactionLine() {TaxRate = 20, TaxCode = "E", LineTotal = 3.5m},
                new TransactionLine() {TaxRate = 25, TaxCode = "F", LineTotal = 0.5m},
                new TransactionLine() {TaxRate = 25, TaxCode = "F", LineTotal = 3m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 0.75m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 39m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 4.99m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 1m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 5m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 10m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 20m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 50m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 100m},
                new TransactionLine() {TaxRate = 0, TaxCode = "A", LineTotal = 1.05m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 5.88m},
                new TransactionLine() {TaxRate = 5, TaxCode = "C", LineTotal = 2.46m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 1.07m},
                new TransactionLine() {TaxRate = 5, TaxCode = "H", LineTotal = 0.72m}
            };

            args.TaxMethod = TaxMethods.ExtractTax;
            args.GlobalDiscountAmount = 31.82m;
            var res = c.Calculate(args);

            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.FirstOrDefault(x => x.TaxRate == 0).TotalTax, 0);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.FirstOrDefault(x => x.TaxRate == 1).TotalTax, 12.06m);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.FirstOrDefault(x => x.TaxRate == 5).TotalTax, 47.65m);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.FirstOrDefault(x => x.TaxRate == 10).TotalTax, 16.38m);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.FirstOrDefault(x => x.TaxRate == 20).TotalTax, 48.46m);
            Assert.AreEqual(res.TaxRateGroups.TaxGroupedByTaxRate.FirstOrDefault(x => x.TaxRate == 25).TotalTax, 1.88m);

            Assert.AreEqual(res.SumLinesWithTax, 2731.82m);
            Assert.AreEqual(res.TotalTax, 126.43m);
            Assert.AreEqual(res.TotalTransactionAfterDiscountWithTax, 2700);
            
            

        }

        [TestMethod]
        public void Complex2()
        {
            CommonTransactionSumCalculator c = new CommonTransactionSumCalculator();
            TransactionCalculatorArgs args = new TransactionCalculatorArgs();
            args.Lines = new[]
            {
                new TransactionLine() {TaxRate = 18, TaxCode = "A", LineTotal = 80m},
                new TransactionLine() {TaxRate = 1, TaxCode = "B", LineTotal = 170m},
                new TransactionLine() {TaxRate = 18, TaxCode = "A", LineTotal = 300m},
            };

            args.TaxMethod = TaxMethods.ExtractTax;
            args.GlobalDiscountAmount = 547m;
            var res = c.Calculate(args);

            //Check that the discount did not change because of rounding issues - This could happen since the discount is distributed for all tax groups.
            Assert.AreEqual(res.TotalDiscountWithTax, 547m);


        }

    }
}
