using Common.App;
using ERP_API.Domin.ProductEntity;

namespace ERP_API.Domin.StockTransferEntity
{
    public class StockTransferLine : Entity
    {
        public int StockTransferId { get; set; }
        public StockTransfer StockTransfer { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        /// <summary>Quantity in base units.</summary>
        public int Quantity { get; set; }
    }
}
