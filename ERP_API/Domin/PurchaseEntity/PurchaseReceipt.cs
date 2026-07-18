using Common.App;
using ERP_API.Domin.SuppliersEntity;
using ERP_API.Domin.WarehouseEntity;

namespace ERP_API.Domin.PurchaseEntity
{
    public class PurchaseReceipt : Entity
    {
        public string ReceiptNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public Suppliers? Supplier { get; set; }

        /// <summary>Optional supplier's own invoice / delivery note number.</summary>
        public string? SupplierInvoiceNumber { get; set; }

        public double SubTotal { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Completed";

        /// <summary>Warehouse stock is increased into for this receipt.</summary>
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public ICollection<PurchaseReceiptLine> Lines { get; set; } = new List<PurchaseReceiptLine>();
    }
}
