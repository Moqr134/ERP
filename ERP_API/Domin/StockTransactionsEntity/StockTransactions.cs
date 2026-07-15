using Common.App;
using ERP_API.Domin.WarehouseEntity;

namespace ERP_API.Domin.StockTransactionsEntity
{
    public class StockTransactions : Entity
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = string.Empty;

        /// <summary>Warehouse this movement affects.</summary>
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        /// <summary>For transfers: the other warehouse in the pair.</summary>
        public int? RelatedWarehouseId { get; set; }

        public string ReferenceId { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
