using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.WarehouseEntity;
using Infrastructure.ORM;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Inventory
{
    /// <summary>
    /// Adjusts per-warehouse balances and keeps Product.CurrentStock = sum of balances.
    /// Call inside an open DB transaction.
    /// </summary>
    public static class WarehouseStockHelper
    {
        public static async Task EnsureWarehouseActiveAsync(DBContext context, int warehouseId)
        {
            var ok = await context.Warehouses.AnyAsync(w => w.Id == warehouseId && !w.IsRemoved && w.IsActive);
            if (!ok)
                throw new KeyNotFoundException("المخزن غير موجود أو غير نشط");
        }

        public static async Task<int> GetQuantityAsync(DBContext context, int productId, int warehouseId)
        {
            return await context.WarehouseStocks
                .Where(s => s.ProductId == productId && s.WarehouseId == warehouseId && !s.IsRemoved)
                .Select(s => s.Quantity)
                .FirstOrDefaultAsync();
        }

        public static async Task<WarehouseStock> GetOrCreateAsync(
            DBContext context,
            Product product,
            int warehouseId,
            int userId,
            DateTime now)
        {
            var stock = await context.WarehouseStocks
                .FirstOrDefaultAsync(s => s.ProductId == product.Id && s.WarehouseId == warehouseId && !s.IsRemoved);

            if (stock is not null)
                return stock;

            stock = new WarehouseStock
            {
                ProductId = product.Id,
                WarehouseId = warehouseId,
                Quantity = 0,
                CreateDate = now,
                CreateUserId = userId
            };
            context.WarehouseStocks.Add(stock);
            return stock;
        }

        /// <summary>delta positive = In, negative = Out.</summary>
        public static async Task ApplyDeltaAsync(
            DBContext context,
            Product product,
            int warehouseId,
            int delta,
            int userId,
            DateTime now,
            string productNameForError)
        {
            if (delta == 0) return;

            var stock = await GetOrCreateAsync(context, product, warehouseId, userId, now);

            var next = checked(stock.Quantity + delta);
            if (next < 0)
                throw new InvalidOperationException($"المخزون غير كافٍ في المخزن للمنتج: {productNameForError}");

            stock.Quantity = next;
            stock.UpdateDate = now;
            stock.UpdateUserId = userId;
            context.WarehouseStocks.Entry(stock).State = EntityState.Modified;

            product.CurrentStock = checked(product.CurrentStock + delta);
            if (product.CurrentStock < 0)
                throw new InvalidOperationException($"المخزون الإجمالي غير كافٍ للمنتج: {productNameForError}");

            product.UpdateDate = now;
            product.UpdateUserId = userId;
            context.Products.Entry(product).State = EntityState.Modified;
        }

        public static async Task RecalcProductTotalAsync(DBContext context, Product product)
        {
            var total = await context.WarehouseStocks
                .Where(s => s.ProductId == product.Id && !s.IsRemoved)
                .SumAsync(s => (int?)s.Quantity) ?? 0;
            product.CurrentStock = total;
            context.Products.Entry(product).State = EntityState.Modified;
        }

        public static async Task<int> ResolveDefaultWarehouseIdAsync(DBContext context)
        {
            var main = await context.Warehouses
                .Where(w => !w.IsRemoved && w.IsActive)
                .OrderBy(w => w.Code == "MAIN" ? 0 : 1)
                .ThenBy(w => w.Id)
                .Select(w => w.Id)
                .FirstOrDefaultAsync();

            if (main == 0)
                throw new InvalidOperationException("لا يوجد مخزن نشط. أضف مخزناً أولاً");

            return main;
        }
    }
}
