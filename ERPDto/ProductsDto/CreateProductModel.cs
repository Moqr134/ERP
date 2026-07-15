using Validation.Attribute;

namespace ERPDto.ProductsDto
{
    public class CreateProductModel
    {
        public int Id { get; set; }
        /// <summary>Legacy header barcode. Optional when Units already include barcodes.</summary>
        [StringValidate(3, 50, true)]
        public string Barcode { get; set; } = string.Empty;
        [StringValidate(3, 100, false)]
        public string Name { get; set; } = string.Empty;
        [StringValidate(3, 50, false)]
        public string SKU { get; set; } = string.Empty;
        public double CostPrice { get; set; }
        public double SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int CategoriesId { get; set; }
        public int? WarehouseId { get; set; }

        /// <summary>
        /// Packaging units (مفرد / كارتون…). If empty, server creates a base unit
        /// "مفرد" with Factor=1 and Product.Barcode / SellingPrice.
        /// </summary>
        public List<ProductUnitInputDto> Units { get; set; } = new();
    }
}
