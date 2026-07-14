using Common.App;
using ERP_API.Domin.ProductEntity;

namespace ERP_API.Domin.SalesEntity
{
    public class SaleLine : Entity
    {
        public int SaleId { get; set; }
        public Sale Sale { get; set; } = null!;
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double LineTotal { get; set; }
    }
}
