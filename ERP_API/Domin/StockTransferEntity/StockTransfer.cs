using Common.App;
using ERP_API.Domin.WarehouseEntity;

namespace ERP_API.Domin.StockTransferEntity
{
    public class StockTransfer : Entity
    {
        public string TransferNumber { get; set; } = string.Empty;
        public int FromWarehouseId { get; set; }
        public Warehouse FromWarehouse { get; set; } = null!;
        public int ToWarehouseId { get; set; }
        public Warehouse ToWarehouse { get; set; } = null!;
        public string Status { get; set; } = "Completed";
        public string? Notes { get; set; }
        public ICollection<StockTransferLine> Lines { get; set; } = new List<StockTransferLine>();
    }
}
