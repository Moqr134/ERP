using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.CategoriesEntity;
using ERP_API.Domin.ProductEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using Infrastructure.AppException;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class ProductService : MasterService, IProductService, IScopped
    {
        public ProductService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }
        private async Task<Product?> GetFullProduct(int id)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(x=>x.Id==id && !x.IsRemoved);
            return product;
        }
        private async Task<Product?> GetProductByName(string name)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Name == name && !p.IsRemoved);
            return product;
        }
        private async Task<Product?> GetProductBySKU(string SKU)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(x=>x.SKU == SKU && !x.IsRemoved);
            return product;
        }

        public async Task<Product?> GetProductByBarcode(string Barcode)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(x=>x.Barcode == Barcode && !x.IsRemoved);
            return product;
        }

        private static IQueryable<Product> ApplyProductFilters(IQueryable<Product> query, PageDto pageDto)
        {
            query = query.Where(x => !x.IsRemoved);

            if (pageDto.CategoryId > 0)
                query = query.Where(x => x.CategoriesId == pageDto.CategoryId);

            if (pageDto.WarehouseId > 0)
                query = query.Where(x => x.WarehouseId == pageDto.WarehouseId);

            if (!string.IsNullOrWhiteSpace(pageDto.SearchTerm))
            {
                var term = pageDto.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Name.Contains(term)
                    || x.Barcode.Contains(term)
                    || x.SKU.Contains(term));
            }

            return query;
        }

        private static void NormalizePaging(PageDto pageDto)
        {
            if (pageDto.PageIndex < 1) pageDto.PageIndex = 1;
            if (pageDto.PageSize < 1) pageDto.PageSize = 10;
            if (pageDto.PageSize > 100) pageDto.PageSize = 100;
        }

        public async Task CreateProduct(CreateProductModel product, int userId)
        {
            Categories? categories = await _context.Categories.FindAsync(product.CategoriesId);
            if (categories == null) 
                throw new KeyNotFoundException("الفئة غير موجوده");
            Product? Newproduct = await GetProductBySKU(product.SKU);
            if (Newproduct != null) throw new DuplicateException("هذا الSKU مستخدم بلفعل في منتج اخر");
            Newproduct = await GetProductByBarcode(product.Barcode);
            if (Newproduct != null) throw new DuplicateException("هذا باركود مستخدم بلفعل في منتج اخر");
            Newproduct = await GetProductByName(product.Name);
            if (Newproduct != null) throw new DuplicateException("هذا الاسم مستخدم بالفعل في منتج آخر");
            if (product.CostPrice > product.SellingPrice) throw new LogicException("سعر  البيع لا يمكن ان يكون اقل من سعر الكلفة");
            if (product.MinStockLevel < 0) throw new LogicException("اقل قيمه مخزونه لا يمن ان تكون في السالب");
            if (product.WarehouseId.HasValue && product.WarehouseId.Value > 0)
            {
                bool warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == product.WarehouseId.Value && !w.IsRemoved);
                if (!warehouseExists) throw new KeyNotFoundException("المخزن غير موجود");
            }
            Newproduct = new Product();
            Newproduct = _mapper.Map<Product>(product);
            Newproduct.WarehouseId = product.WarehouseId.HasValue && product.WarehouseId.Value > 0
                ? product.WarehouseId.Value
                : null;
            // Opening stock must go through stock transactions for audit trail
            Newproduct.CurrentStock = 0;
            Newproduct.CreateDate = DateTime.UtcNow.AddHours(3);
            Newproduct.CreateUserId = userId;
            _context.Products.Add(Newproduct);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteProduct(int id, int userId)
        {
            Product? product = await GetFullProduct(id);
            if (product == null) throw new KeyNotFoundException("حدث خطا اثناء جلب البيانات");
            product.RemoveDate = DateTime.UtcNow.AddHours(3);
            product.IsRemoved = true;
            product.RemoveUserId = userId;
            _context.Products.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task<List<ProductDto>> GetAllProductsAsync(PageDto pageDto)
        {
            NormalizePaging(pageDto);
            List<ProductDto> products = await ApplyProductFilters(_context.Products.AsQueryable(), pageDto)
                .OrderByDescending(x => x.Id)
                .Skip((pageDto.PageIndex - 1) * pageDto.PageSize)
                .Take(pageDto.PageSize)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,
                    SKU = x.SKU,
                    SellingPrice = x.SellingPrice,
                    CostPrice = x.CostPrice,
                    CurrentStock = x.CurrentStock,
                    MinStockLevel = x.MinStockLevel,
                    CategoriesId = x.CategoriesId,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
                }).ToListAsync();
            return products;
        }
        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            ProductDto? product = await _context.Products.Where(p => p.Id == id && !p.IsRemoved)
                .Select(x=> new ProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,
                    SKU= x.SKU,
                    SellingPrice = x.SellingPrice,
                    CostPrice = x.CostPrice,
                    CurrentStock=x.CurrentStock,
                    MinStockLevel=x.MinStockLevel,
                    CategoriesId=x.CategoriesId,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
                }).FirstOrDefaultAsync();
            if (product == null) throw new KeyNotFoundException("المنتج غير موجود");
            return product;
        }
        public async Task UpdateProduct(UpdateProductModel product, int userId)
        {
            Product? UpdateProduct = await GetFullProduct(product.Id);
            if (UpdateProduct == null) throw new KeyNotFoundException("المنتج غير موجود");
            if(product.CategoryId != 0 && product.CategoryId != UpdateProduct.CategoriesId)
            {
                Categories? categories = await _context.Categories.FindAsync(product.CategoryId);
                if(categories == null) throw new KeyNotFoundException("الفئة غير موجوده");
                UpdateProduct.CategoriesId = product.CategoryId;
            }
            if(!string.IsNullOrWhiteSpace(product.SKU) && product.SKU != UpdateProduct.SKU)
            {
                Product? SKU = await GetProductBySKU(product.SKU);
                if (SKU != null) throw new DuplicateException("هذا ال SKU مستخدم في منتج اخر");
                UpdateProduct.SKU = product.SKU;
            }
            if(!string.IsNullOrWhiteSpace(product.Barcode) && product.Barcode != UpdateProduct.Barcode)
            {
                Product? Barcode = await GetProductByBarcode(product.Barcode);
                if (Barcode != null) throw new DuplicateException("هذا الباركود مستخدم في منتج اخر");
                UpdateProduct.Barcode = product.Barcode;
            }
            if(!string.IsNullOrWhiteSpace(product.Name) && UpdateProduct.Name != product.Name)
            {
                Product? Name = await GetProductByName(product.Name);
                if (Name != null) throw new DuplicateException("هذا الاسم مستخدم في منتج اخر");
                UpdateProduct.Name = product.Name;
            }

            if (product.WarehouseId.HasValue)
            {
                if (product.WarehouseId.Value > 0)
                {
                    if (product.WarehouseId.Value != UpdateProduct.WarehouseId)
                    {
                        bool warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == product.WarehouseId.Value && !w.IsRemoved);
                        if (!warehouseExists) throw new KeyNotFoundException("المخزن غير موجود");
                        UpdateProduct.WarehouseId = product.WarehouseId.Value;
                    }
                }
                else
                {
                    UpdateProduct.WarehouseId = null;
                }
            }

            if (product.CostPrice.HasValue && product.CostPrice.Value < 0)
                throw new LogicException("سعر الكلفة لا يمكن أن يكون سالباً");
            if (product.Price < 0)
                throw new LogicException("سعر البيع لا يمكن أن يكون سالباً");
            if (product.MinStockLevel.HasValue && product.MinStockLevel.Value < 0)
                throw new LogicException("اقل قيمة مخزونة لا يمكن أن تكون سالبة");

            var newCost = product.CostPrice ?? UpdateProduct.CostPrice;
            var newPrice = product.Price;
            if (newCost > newPrice)
                throw new LogicException("سعر البيع لا يمكن ان يكون اقل من سعر الكلفة");

            UpdateProduct.UpdateDate = DateTime.UtcNow.AddHours(3);
            UpdateProduct.UpdateUserId = userId;
            UpdateProduct.SellingPrice = newPrice;
            if (product.CostPrice.HasValue)
                UpdateProduct.CostPrice = product.CostPrice.Value;
            if (product.MinStockLevel.HasValue)
                UpdateProduct.MinStockLevel = product.MinStockLevel.Value;

            _context.Products.Entry(UpdateProduct).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductStockLadgerDto>> GetProductStockLedger(int id)
        {
            Product? product = await GetFullProduct(id);
            if (product == null) throw new KeyNotFoundException("لم يتم العثور على المنتج");
            List<ProductStockLadgerDto> dto = await _context.StockTransactions.Where(s => s.ProductId == id)
               .Select(x => new ProductStockLadgerDto
               {
                   Notes = x.Notes,
                   CreateDate = x.CreateDate,
                   Quantity = x.Quantity,
                   TransactionType = x.TransactionType,
                   ReferenceId = x.ReferenceId,
                   ProductName = product.Name,
               }).ToListAsync();
            return dto;
        }

        public async Task<List<ProductDto>> GetLowStockProduct()
        {
            List<ProductDto> products = await _context.Products.Where(x => !x.IsRemoved && x.CurrentStock <= x.MinStockLevel)
                .Select(x => new ProductDto
                {
                    Name = x.Name,
                    Barcode = x.Barcode,
                    CostPrice = x.CostPrice,
                    CurrentStock = x.CurrentStock,
                    Id = x.Id,
                    MinStockLevel = x.MinStockLevel,
                    SellingPrice = x.SellingPrice,
                    SKU = x.SKU,
                    CategoriesId = x.CategoriesId,
                }).ToListAsync();
            return products;
        }

        public async Task<ProductsInfo> GetProductsInfo(PageDto pageDto)
        {
            NormalizePaging(pageDto);
            var filtered = ApplyProductFilters(_context.Products.AsQueryable(), pageDto);

            ProductsInfo productsInfo = new ProductsInfo();
            productsInfo.TotalProducts = await filtered.CountAsync();
            productsInfo.ProductsStockOut = await filtered.CountAsync(p => p.CurrentStock == 0);
            productsInfo.ProductsCountLissMinStock = await filtered.CountAsync(p => p.CurrentStock < p.MinStockLevel);
            productsInfo.ProductsCostCount = await filtered.Where(p => p.CurrentStock > 0).SumAsync(p => (double?)(p.CostPrice * p.CurrentStock)) ?? 0;
            productsInfo.PageCount = (int)Math.Ceiling((double)productsInfo.TotalProducts / pageDto.PageSize);
            return productsInfo;
        }
    }
}
