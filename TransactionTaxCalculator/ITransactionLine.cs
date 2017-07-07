namespace TransactionTaxCalculator
{
    public interface ITransactionLine
    {
        long LineID { get; }
        decimal Qty { get; }
        string TaxCode { get; }
        decimal TaxRate { get; }
        decimal LineTotal { get; }
    }

    public class TransactionLine : ITransactionLine
    {
        public long LineID { get; set; }
        public decimal Qty { get; set; }
        public string TaxCode { get; set; }
        public decimal TaxRate { get; set; }
        public decimal LineTotal { get; set; }
    }
}