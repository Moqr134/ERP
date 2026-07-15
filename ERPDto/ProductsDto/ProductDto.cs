namespace ERPDto.ProductsDto
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public double CostPrice { get; set; }
        public double SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int CategoriesId { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public List<ProductUnitDto> Units { get; set; } = new();
    }
}
