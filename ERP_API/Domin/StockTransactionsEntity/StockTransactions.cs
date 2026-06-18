using Common.App;
using ERP_API.Domin.ProductEntity;

namespace ERP_API.Domin.StockTransactionsEntity
{
    public class StockTransactions:Entity
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; }
        public string ReferenceId { get; set; }
        public string Notes { get; set; }
    }
}
