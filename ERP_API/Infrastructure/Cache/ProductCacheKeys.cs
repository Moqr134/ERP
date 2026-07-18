using ERPDto.PaigingDto;
using ERPDto.ProductsDto;

namespace ERP_API.Infrastructure.Cache
{
    public static class ProductCacheKeys
    {
        public static string PageKey(string prefix, PageDto pageDto)
        {
            var term = (pageDto.SearchTerm ?? string.Empty).Trim().ToLowerInvariant();
            return $"{prefix}:{pageDto.PageIndex}:{pageDto.PageSize}:{term}:c{pageDto.CategoryId}:w{pageDto.WarehouseId}";
        }

        public static string ById(int id) => $"by-id:{id}";
        public static string Lookup(string barcode) => $"lookup:{barcode.Trim().ToLowerInvariant()}";
        public static string LowStock() => "low-stock";
        public static string Search(string term, int take, int? warehouseId)
            => $"search:{(term ?? string.Empty).Trim().ToLowerInvariant()}:t{take}:w{warehouseId ?? 0}";

        public static ProductLookupDto CloneLookup(ProductLookupDto src) => new()
        {
            ProductId = src.ProductId,
            Name = src.Name,
            SKU = src.SKU,
            CurrentStock = src.CurrentStock,
            MinStockLevel = src.MinStockLevel,
            CategoriesId = src.CategoriesId,
            WarehouseId = src.WarehouseId,
            CostPrice = src.CostPrice,
            UnitId = src.UnitId,
            UnitName = src.UnitName,
            UnitFactor = src.UnitFactor,
            UnitPrice = src.UnitPrice,
            Barcode = src.Barcode
        };
    }
}
