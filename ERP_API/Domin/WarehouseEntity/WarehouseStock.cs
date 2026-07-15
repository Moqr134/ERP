using Common.App;
using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.WarehouseEntity;

namespace ERP_API.Domin.WarehouseEntity
{
    /// <summary>
    /// Quantity of a product in a specific warehouse (base units).
    /// Product.CurrentStock remains the company-wide sum for compatibility.
    /// </summary>
    public class WarehouseStock : Entity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        /// <summary>Stock in base units (Factor = 1).</summary>
        public int Quantity { get; set; }
    }
}
