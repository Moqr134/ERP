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
        [StringValidate(3, 50, false)]
        public string Barcode { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}
