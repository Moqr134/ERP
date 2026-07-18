using Common.App;
using ERP_API.Domin.WarehouseEntity;

namespace ERP_API.Domin.SalesEntity
{
    public class Sale : Entity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash";
        public double SubTotal { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public double PaidAmount { get; set; }
        public double ChangeAmount { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Completed";

        /// <summary>Warehouse stock is deducted from for this sale.</summary>
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
    }
}
