using System.ComponentModel.DataAnnotations;
using Validation.Attribute;

namespace ERPDto.ProductsDto
{
    public class UpdateProductModel
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
        [StringValidate(3, 100, false)]
        public string Name { get; set; } = string.Empty;
        [StringValidate(3, 50, false)]
        public string SKU { get; set; } = string.Empty;
        [Range(0, double.MaxValue)]
        public double Price { get; set; }
        public double? CostPrice { get; set; }
        public int? MinStockLevel { get; set; }
        /// <summary>Legacy header barcode. Optional when Units already include barcodes.</summary>
        [StringValidate(3, 50, true)]
        public string Barcode { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
        public int? WarehouseId { get; set; }

        /// <summary>
        /// When provided (non-null), replaces the product's unit/barcode structure.
        /// Omit or leave empty to keep existing units and only update header fields.
        /// </summary>
        public List<ProductUnitInputDto>? Units { get; set; }
    }
}
