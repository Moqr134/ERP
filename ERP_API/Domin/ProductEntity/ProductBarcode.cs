using Common.App;

namespace ERP_API.Domin.ProductEntity
{
    /// <summary>
    /// One of potentially many barcodes for a product unit.
    /// Barcodes must be unique among non-removed rows.
    /// </summary>
    public class ProductBarcode : Entity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int ProductUnitId { get; set; }
        public ProductUnit ProductUnit { get; set; } = null!;

        public string Barcode { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
