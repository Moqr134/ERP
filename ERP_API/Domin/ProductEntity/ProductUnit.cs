using Common.App;

namespace ERP_API.Domin.ProductEntity
{
    /// <summary>
    /// Sellable unit/package for a product (e.g. مفرد factor=1, كارتون factor=24).
    /// Stock on Product.CurrentStock is always stored in base units (Factor=1).
    /// </summary>
    public class ProductUnit : Entity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        /// <summary>Display name: مفرد، كارتون، باكيت…</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>How many base units this package equals. Base unit must be 1.</summary>
        public int Factor { get; set; } = 1;

        public double SellingPrice { get; set; }
        public bool IsBase { get; set; }
        public bool IsDefaultForSale { get; set; }
        public int SortOrder { get; set; }

        public ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();
    }
}
