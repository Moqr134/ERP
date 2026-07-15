using Common.App;
using ERP_API.Domin.CategoriesEntity;
using ERP_API.Domin.WarehouseEntity;

namespace ERP_API.Domin.ProductEntity
{
    public class Product : Entity
    {
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public double CostPrice { get; set; }
        public double SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int CategoriesId { get; set; }
        public Categories Categories { get; set; } = null!;
        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public ICollection<ProductUnit> Units { get; set; } = new List<ProductUnit>();
        public ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();
    }
}
