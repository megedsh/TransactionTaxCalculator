using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TransactionTaxCalculator;
using TransactionTaxCalculator.TaxCalculationStratagies.DefaultStratagy;

namespace TransactionTaxCalculator_Tests
{
    [TestClass]
    public class CommonTests
    {
        [TestMethod]
        public void TestMultipleTaxCodeValidationError()
        {

            TransactionCalculatorArgs args = new TransactionCalculatorArgs();
            args.Lines = new[]
            {
                new TransactionLine() {TaxRate = 18, TaxCode = "A", LineTotal = 80m},
                new TransactionLine() {TaxRate = 12, TaxCode = "A", LineTotal = 300m},
                new TransactionLine() {TaxRate = 1, TaxCode = "B", LineTotal = 170m},
            };

            TransactionTaxCalculator.TransactionTaxCalculator c = new TransactionTaxCalculator.TransactionTaxCalculator(DefaultStratagy_ExtractTax.Instance);
            var res = c.Calculate(args);

            Assert.IsFalse(res.Success);
            Assert.IsNotNull(res.Exception);
        }
    }
}
