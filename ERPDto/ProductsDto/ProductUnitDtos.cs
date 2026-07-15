namespace ERPDto.ProductsDto
{
    public class ProductBarcodeDto
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class ProductUnitDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Factor { get; set; } = 1;
        public double SellingPrice { get; set; }
        public bool IsBase { get; set; }
        public bool IsDefaultForSale { get; set; }
        public int SortOrder { get; set; }
        public List<ProductBarcodeDto> Barcodes { get; set; } = new();
    }

    /// <summary>Input model for creating/updating a product unit with its barcodes.</summary>
    public class ProductUnitInputDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Factor { get; set; } = 1;
        public double SellingPrice { get; set; }
        public bool IsBase { get; set; }
        public bool IsDefaultForSale { get; set; }
        public int SortOrder { get; set; }

        /// <summary>At least one barcode. First primary barcode for the unit can be marked IsPrimary.</summary>
        public List<ProductBarcodeInputDto> Barcodes { get; set; } = new();
    }

    public class ProductBarcodeInputDto
    {
        public int? Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    /// <summary>POS / barcode resolution result including unit packaging.</summary>
    public class ProductLookupDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int CategoriesId { get; set; }
        public int? WarehouseId { get; set; }
        public double CostPrice { get; set; }

        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int UnitFactor { get; set; } = 1;
        public double UnitPrice { get; set; }
        public string Barcode { get; set; } = string.Empty;

        /// <summary>How many whole packages can be sold from current base stock.</summary>
        public int AvailablePackages => UnitFactor <= 0 ? 0 : CurrentStock / UnitFactor;
    }
}
