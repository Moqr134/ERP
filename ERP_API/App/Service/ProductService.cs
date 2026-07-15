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
            => await _context.Products
                .Include(p => p.Units.Where(u => !u.IsRemoved))
                    .ThenInclude(u => u.Barcodes.Where(b => !b.IsRemoved))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsRemoved);

        private async Task<Product?> GetProductByName(string name)
            => await _context.Products.FirstOrDefaultAsync(p => p.Name == name && !p.IsRemoved);

        private async Task<Product?> GetProductBySKU(string sku)
            => await _context.Products.FirstOrDefaultAsync(x => x.SKU == sku && !x.IsRemoved);

        public async Task<Product?> GetProductByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return null;

            var term = barcode.Trim();
            var viaUnit = await _context.ProductBarcodes
                .AsNoTracking()
                .Include(b => b.Product)
                .FirstOrDefaultAsync(b => b.Barcode == term && !b.IsRemoved && b.Product != null && !b.Product.IsRemoved);

            if (viaUnit?.Product is not null)
                return viaUnit.Product;

            return await _context.Products.FirstOrDefaultAsync(x => x.Barcode == term && !x.IsRemoved);
        }

        public async Task<ProductLookupDto?> LookupByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return null;

            var term = barcode.Trim();
            var hit = await _context.ProductBarcodes
                .AsNoTracking()
                .Include(b => b.ProductUnit)
                .Include(b => b.Product)
                .FirstOrDefaultAsync(b =>
                    b.Barcode == term
                    && !b.IsRemoved
                    && b.Product != null && !b.Product.IsRemoved
                    && b.ProductUnit != null && !b.ProductUnit.IsRemoved);

            if (hit is not null)
                return ToLookup(hit.Product!, hit.ProductUnit!, hit.Barcode);

            // Legacy fallback: product header barcode → base/default unit
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Units.Where(u => !u.IsRemoved))
                .FirstOrDefaultAsync(p => p.Barcode == term && !p.IsRemoved);

            if (product is null)
                return null;

            var unit = product.Units.FirstOrDefault(u => u.IsDefaultForSale)
                ?? product.Units.FirstOrDefault(u => u.IsBase)
                ?? product.Units.OrderBy(u => u.SortOrder).FirstOrDefault();

            if (unit is null)
            {
                return new ProductLookupDto
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    SKU = product.SKU,
                    CurrentStock = product.CurrentStock,
                    MinStockLevel = product.MinStockLevel,
                    CategoriesId = product.CategoriesId,
                    WarehouseId = product.WarehouseId,
                    CostPrice = product.CostPrice,
                    UnitId = 0,
                    UnitName = "مفرد",
                    UnitFactor = 1,
                    UnitPrice = product.SellingPrice,
                    Barcode = product.Barcode
                };
            }

            return ToLookup(product, unit, term);
        }

        private static ProductLookupDto ToLookup(Product product, ProductUnit unit, string barcode) => new()
        {
            ProductId = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            CurrentStock = product.CurrentStock,
            MinStockLevel = product.MinStockLevel,
            CategoriesId = product.CategoriesId,
            WarehouseId = product.WarehouseId,
            CostPrice = product.CostPrice,
            UnitId = unit.Id,
            UnitName = unit.Name,
            UnitFactor = unit.Factor,
            UnitPrice = unit.SellingPrice,
            Barcode = barcode
        };

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
                    || x.SKU.Contains(term)
                    || x.Barcodes.Any(b => !b.IsRemoved && b.Barcode.Contains(term)));
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

            if (await GetProductBySKU(product.SKU) != null)
                throw new DuplicateException("هذا الSKU مستخدم بلفعل في منتج اخر");
            if (await GetProductByName(product.Name) != null)
                throw new DuplicateException("هذا الاسم مستخدم بالفعل في منتج آخر");
            if (product.CostPrice > product.SellingPrice)
                throw new LogicException("سعر  البيع لا يمكن ان يكون اقل من سعر الكلفة");
            if (product.MinStockLevel < 0)
                throw new LogicException("اقل قيمه مخزونه لا يمن ان تكون في السالب");

            if (product.WarehouseId.HasValue && product.WarehouseId.Value > 0)
            {
                bool warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == product.WarehouseId.Value && !w.IsRemoved);
                if (!warehouseExists) throw new KeyNotFoundException("المخزن غير موجود");
            }

            var units = NormalizeUnitInputs(product.Units, product.Barcode, product.SellingPrice);
            await EnsureBarcodesAvailableAsync(units.SelectMany(u => u.Barcodes.Select(b => b.Barcode)), excludeProductId: null);

            var now = DateTime.UtcNow.AddHours(3);
            var entity = _mapper.Map<Product>(product);
            entity.WarehouseId = product.WarehouseId is > 0 ? product.WarehouseId : null;
            entity.CurrentStock = 0;
            entity.CreateDate = now;
            entity.CreateUserId = userId;
            entity.Barcode = GetPrimaryBarcode(units);
            entity.SellingPrice = units.First(u => u.IsBase).SellingPrice;

            ApplyUnitsToProduct(entity, units, userId, now, isNew: true);

            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProduct(int id, int userId)
        {
            Product? product = await GetFullProduct(id);
            if (product == null) throw new KeyNotFoundException("حدث خطا اثناء جلب البيانات");

            var now = DateTime.UtcNow.AddHours(3);
            product.RemoveDate = now;
            product.IsRemoved = true;
            product.RemoveUserId = userId;

            foreach (var unit in product.Units)
            {
                unit.IsRemoved = true;
                unit.RemoveDate = now;
                unit.RemoveUserId = userId;
                foreach (var barcode in unit.Barcodes)
                {
                    barcode.IsRemoved = true;
                    barcode.RemoveDate = now;
                    barcode.RemoveUserId = userId;
                }
            }

            _context.Products.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductDto>> GetAllProductsAsync(PageDto pageDto)
        {
            NormalizePaging(pageDto);
            var products = await ApplyProductFilters(_context.Products.AsQueryable(), pageDto)
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
                })
                .ToListAsync();

            // Attach units for the page (keeps list query light, second query by ids)
            var ids = products.Select(p => p.Id).ToList();
            if (ids.Count > 0)
            {
                var units = await LoadUnitDtosByProductIdsAsync(ids);
                foreach (var p in products)
                    p.Units = units.TryGetValue(p.Id, out var list) ? list : new();
            }

            return products;
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id && !p.IsRemoved)
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
                })
                .FirstOrDefaultAsync();

            if (product == null) throw new KeyNotFoundException("المنتج غير موجود");

            var units = await LoadUnitDtosByProductIdsAsync([id]);
            product.Units = units.TryGetValue(id, out var list) ? list : new();
            return product;
        }

        public async Task UpdateProduct(UpdateProductModel product, int userId)
        {
            Product? updateProduct = await GetFullProduct(product.Id);
            if (updateProduct == null) throw new KeyNotFoundException("المنتج غير موجود");

            if (product.CategoryId != 0 && product.CategoryId != updateProduct.CategoriesId)
            {
                Categories? categories = await _context.Categories.FindAsync(product.CategoryId);
                if (categories == null) throw new KeyNotFoundException("الفئة غير موجوده");
                updateProduct.CategoriesId = product.CategoryId;
            }

            if (!string.IsNullOrWhiteSpace(product.SKU) && product.SKU != updateProduct.SKU)
            {
                if (await GetProductBySKU(product.SKU) != null)
                    throw new DuplicateException("هذا ال SKU مستخدم في منتج اخر");
                updateProduct.SKU = product.SKU;
            }

            if (!string.IsNullOrWhiteSpace(product.Name) && updateProduct.Name != product.Name)
            {
                if (await GetProductByName(product.Name) != null)
                    throw new DuplicateException("هذا الاسم مستخدم في منتج اخر");
                updateProduct.Name = product.Name;
            }

            if (product.WarehouseId.HasValue)
            {
                if (product.WarehouseId.Value > 0)
                {
                    if (product.WarehouseId.Value != updateProduct.WarehouseId)
                    {
                        bool warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == product.WarehouseId.Value && !w.IsRemoved);
                        if (!warehouseExists) throw new KeyNotFoundException("المخزن غير موجود");
                        updateProduct.WarehouseId = product.WarehouseId.Value;
                    }
                }
                else
                {
                    updateProduct.WarehouseId = null;
                }
            }

            if (product.CostPrice.HasValue && product.CostPrice.Value < 0)
                throw new LogicException("سعر الكلفة لا يمكن أن يكون سالباً");
            if (product.Price < 0)
                throw new LogicException("سعر البيع لا يمكن أن يكون سالباً");
            if (product.MinStockLevel.HasValue && product.MinStockLevel.Value < 0)
                throw new LogicException("اقل قيمة مخزونة لا يمكن أن تكون سالبة");

            var newCost = product.CostPrice ?? updateProduct.CostPrice;
            var newPrice = product.Price;
            if (newCost > newPrice)
                throw new LogicException("سعر البيع لا يمكن ان يكون اقل من سعر الكلفة");

            var now = DateTime.UtcNow.AddHours(3);
            updateProduct.UpdateDate = now;
            updateProduct.UpdateUserId = userId;
            updateProduct.SellingPrice = newPrice;
            if (product.CostPrice.HasValue)
                updateProduct.CostPrice = product.CostPrice.Value;
            if (product.MinStockLevel.HasValue)
                updateProduct.MinStockLevel = product.MinStockLevel.Value;

            if (product.Units is { Count: > 0 })
            {
                var units = NormalizeUnitInputs(product.Units, product.Barcode, product.Price);
                await EnsureBarcodesAvailableAsync(
                    units.SelectMany(u => u.Barcodes.Select(b => b.Barcode)),
                    excludeProductId: updateProduct.Id);

                SoftRemoveMissingUnits(updateProduct, units, userId, now);
                ApplyUnitsToProduct(updateProduct, units, userId, now, isNew: false);
                updateProduct.Barcode = GetPrimaryBarcode(units);
                updateProduct.SellingPrice = units.First(u => u.IsBase).SellingPrice;
            }
            else if (!string.IsNullOrWhiteSpace(product.Barcode) && product.Barcode != updateProduct.Barcode)
            {
                // Legacy single-barcode edit: update header + primary base barcode if present
                await EnsureBarcodesAvailableAsync([product.Barcode], excludeProductId: updateProduct.Id);
                updateProduct.Barcode = product.Barcode.Trim();
                var baseUnit = updateProduct.Units.FirstOrDefault(u => u.IsBase)
                    ?? updateProduct.Units.FirstOrDefault();
                if (baseUnit is not null)
                {
                    var primary = baseUnit.Barcodes.FirstOrDefault(b => b.IsPrimary)
                        ?? baseUnit.Barcodes.FirstOrDefault();
                    if (primary is not null)
                    {
                        primary.Barcode = updateProduct.Barcode;
                        primary.UpdateDate = now;
                        primary.UpdateUserId = userId;
                    }
                    else
                    {
                        baseUnit.Barcodes.Add(new ProductBarcode
                        {
                            ProductId = updateProduct.Id,
                            Barcode = updateProduct.Barcode,
                            IsPrimary = true,
                            CreateDate = now,
                            CreateUserId = userId
                        });
                    }
                    baseUnit.SellingPrice = newPrice;
                }
            }

            _context.Products.Entry(updateProduct).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductStockLadgerDto>> GetProductStockLedger(int id)
        {
            Product? product = await GetFullProduct(id);
            if (product == null) throw new KeyNotFoundException("لم يتم العثور على المنتج");
            return await _context.StockTransactions.Where(s => s.ProductId == id)
               .Select(x => new ProductStockLadgerDto
               {
                   Notes = x.Notes,
                   CreateDate = x.CreateDate,
                   Quantity = x.Quantity,
                   TransactionType = x.TransactionType,
                   ReferenceId = x.ReferenceId,
                   ProductName = product.Name,
               }).ToListAsync();
        }

        public async Task<List<ProductDto>> GetLowStockProduct()
        {
            return await _context.Products.Where(x => !x.IsRemoved && x.CurrentStock <= x.MinStockLevel)
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
                    WarehouseId = x.WarehouseId,
                }).ToListAsync();
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

        private async Task<Dictionary<int, List<ProductUnitDto>>> LoadUnitDtosByProductIdsAsync(List<int> productIds)
        {
            var rows = await _context.ProductUnits
                .AsNoTracking()
                .Where(u => productIds.Contains(u.ProductId) && !u.IsRemoved)
                .OrderBy(u => u.SortOrder)
                .ThenBy(u => u.Id)
                .Select(u => new
                {
                    u.ProductId,
                    Unit = new ProductUnitDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Factor = u.Factor,
                        SellingPrice = u.SellingPrice,
                        IsBase = u.IsBase,
                        IsDefaultForSale = u.IsDefaultForSale,
                        SortOrder = u.SortOrder,
                        Barcodes = u.Barcodes
                            .Where(b => !b.IsRemoved)
                            .OrderByDescending(b => b.IsPrimary)
                            .ThenBy(b => b.Id)
                            .Select(b => new ProductBarcodeDto
                            {
                                Id = b.Id,
                                Barcode = b.Barcode,
                                IsPrimary = b.IsPrimary
                            })
                            .ToList()
                    }
                })
                .ToListAsync();

            return rows
                .GroupBy(r => r.ProductId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Unit).ToList());
        }

        private static List<ProductUnitInputDto> NormalizeUnitInputs(
            List<ProductUnitInputDto>? inputs,
            string fallbackBarcode,
            double fallbackPrice)
        {
            var list = (inputs ?? new())
                .Where(u => !string.IsNullOrWhiteSpace(u.Name))
                .Select(u =>
                {
                    u.Name = u.Name.Trim();
                    u.Barcodes = (u.Barcodes ?? new())
                        .Where(b => !string.IsNullOrWhiteSpace(b.Barcode))
                        .Select(b =>
                        {
                            b.Barcode = b.Barcode.Trim();
                            return b;
                        })
                        .GroupBy(b => b.Barcode, StringComparer.OrdinalIgnoreCase)
                        .Select(g => g.First())
                        .ToList();
                    return u;
                })
                .ToList();

            if (list.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(fallbackBarcode))
                    throw new LogicException("يجب إدخال باركود واحد على الأقل");

                list.Add(new ProductUnitInputDto
                {
                    Name = "مفرد",
                    Factor = 1,
                    SellingPrice = fallbackPrice,
                    IsBase = true,
                    IsDefaultForSale = true,
                    SortOrder = 0,
                    Barcodes =
                    [
                        new ProductBarcodeInputDto { Barcode = fallbackBarcode.Trim(), IsPrimary = true }
                    ]
                });
            }

            // Exactly one base unit with Factor = 1
            if (list.Count(u => u.IsBase) == 0)
            {
                var piece = list.FirstOrDefault(u => u.Factor == 1) ?? list[0];
                piece.IsBase = true;
                piece.Factor = 1;
            }

            if (list.Count(u => u.IsBase) != 1)
                throw new LogicException("يجب تحديد وحدة أساس واحدة فقط (مفرد)");

            var baseUnit = list.First(u => u.IsBase);
            if (baseUnit.Factor != 1)
                throw new LogicException("وحدة الأساس يجب أن يكون معاملها 1");

            if (list.Count(u => u.IsDefaultForSale) == 0)
                baseUnit.IsDefaultForSale = true;
            if (list.Count(u => u.IsDefaultForSale) != 1)
                throw new LogicException("يجب تحديد وحدة بيع افتراضية واحدة فقط");

            foreach (var unit in list)
            {
                if (unit.Factor < 1)
                    throw new LogicException($"معامل الوحدة «{unit.Name}» غير صحيح");
                if (unit.SellingPrice < 0)
                    throw new LogicException($"سعر الوحدة «{unit.Name}» غير صحيح");
                if (unit.Barcodes.Count == 0)
                    throw new LogicException($"الوحدة «{unit.Name}» تحتاج باركود واحد على الأقل");

                if (!unit.Barcodes.Any(b => b.IsPrimary))
                    unit.Barcodes[0].IsPrimary = true;
                if (unit.Barcodes.Count(b => b.IsPrimary) != 1)
                    throw new LogicException($"الوحدة «{unit.Name}» يجب أن تحتوي باركود أساسي واحد فقط");
            }

            var nameDup = list.GroupBy(u => u.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (nameDup is not null)
                throw new LogicException($"تكرار اسم الوحدة: {nameDup.Key}");

            var allBarcodes = list.SelectMany(u => u.Barcodes.Select(b => b.Barcode)).ToList();
            var dupBarcode = allBarcodes.GroupBy(b => b, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (dupBarcode is not null)
                throw new LogicException($"الباركود مكرر داخل المنتج: {dupBarcode.Key}");

            for (var i = 0; i < list.Count; i++)
                if (list[i].SortOrder == 0 && i > 0)
                    list[i].SortOrder = i;

            return list;
        }

        private async Task EnsureBarcodesAvailableAsync(IEnumerable<string> barcodes, int? excludeProductId)
        {
            var list = barcodes.Select(b => b.Trim()).Where(b => b.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (list.Count == 0) return;

            var takenInBarcodes = await _context.ProductBarcodes
                .Where(b => !b.IsRemoved && list.Contains(b.Barcode)
                    && (!excludeProductId.HasValue || b.ProductId != excludeProductId.Value))
                .Select(b => b.Barcode)
                .ToListAsync();

            var takenOnHeader = await _context.Products
                .Where(p => !p.IsRemoved && list.Contains(p.Barcode)
                    && (!excludeProductId.HasValue || p.Id != excludeProductId.Value))
                .Select(p => p.Barcode)
                .ToListAsync();

            var conflict = takenInBarcodes.Concat(takenOnHeader).FirstOrDefault();
            if (conflict is not null)
                throw new DuplicateException($"الباركود مستخدم بالفعل: {conflict}");
        }

        private static string GetPrimaryBarcode(List<ProductUnitInputDto> units)
        {
            var baseUnit = units.First(u => u.IsBase);
            return baseUnit.Barcodes.First(b => b.IsPrimary).Barcode;
        }

        private void SoftRemoveMissingUnits(Product product, List<ProductUnitInputDto> desired, int userId, DateTime now)
        {
            var keepIds = desired.Where(u => u.Id.HasValue && u.Id.Value > 0).Select(u => u.Id!.Value).ToHashSet();
            foreach (var unit in product.Units.Where(u => !u.IsRemoved && !keepIds.Contains(u.Id)))
            {
                unit.IsRemoved = true;
                unit.RemoveDate = now;
                unit.RemoveUserId = userId;
                foreach (var barcode in unit.Barcodes.Where(b => !b.IsRemoved))
                {
                    barcode.IsRemoved = true;
                    barcode.RemoveDate = now;
                    barcode.RemoveUserId = userId;
                }
            }
        }

        private void ApplyUnitsToProduct(Product product, List<ProductUnitInputDto> units, int userId, DateTime now, bool isNew)
        {
            foreach (var input in units)
            {
                ProductUnit unit;
                if (!isNew && input.Id is > 0)
                {
                    unit = product.Units.FirstOrDefault(u => u.Id == input.Id.Value)
                        ?? throw new KeyNotFoundException($"وحدة المنتج غير موجودة: {input.Id}");
                    unit.Name = input.Name;
                    unit.Factor = input.Factor;
                    unit.SellingPrice = input.SellingPrice;
                    unit.IsBase = input.IsBase;
                    unit.IsDefaultForSale = input.IsDefaultForSale;
                    unit.SortOrder = input.SortOrder;
                    unit.UpdateDate = now;
                    unit.UpdateUserId = userId;
                    unit.IsRemoved = false;
                }
                else
                {
                    unit = new ProductUnit
                    {
                        Name = input.Name,
                        Factor = input.Factor,
                        SellingPrice = input.SellingPrice,
                        IsBase = input.IsBase,
                        IsDefaultForSale = input.IsDefaultForSale,
                        SortOrder = input.SortOrder,
                        CreateDate = now,
                        CreateUserId = userId
                    };
                    product.Units.Add(unit);
                }

                SyncUnitBarcodes(product, unit, input.Barcodes, userId, now);
            }
        }

        private void SyncUnitBarcodes(
            Product product,
            ProductUnit unit,
            List<ProductBarcodeInputDto> desired,
            int userId,
            DateTime now)
        {
            var keepIds = desired.Where(b => b.Id.HasValue && b.Id.Value > 0).Select(b => b.Id!.Value).ToHashSet();
            foreach (var existing in unit.Barcodes.Where(b => !b.IsRemoved && !keepIds.Contains(b.Id)))
            {
                existing.IsRemoved = true;
                existing.RemoveDate = now;
                existing.RemoveUserId = userId;
            }

            foreach (var input in desired)
            {
                if (input.Id is > 0)
                {
                    var row = unit.Barcodes.FirstOrDefault(b => b.Id == input.Id.Value)
                        ?? throw new KeyNotFoundException("باركود غير موجود");
                    row.Barcode = input.Barcode;
                    row.IsPrimary = input.IsPrimary;
                    row.UpdateDate = now;
                    row.UpdateUserId = userId;
                    row.IsRemoved = false;
                }
                else
                {
                    unit.Barcodes.Add(new ProductBarcode
                    {
                        Product = product,
                        Barcode = input.Barcode,
                        IsPrimary = input.IsPrimary,
                        CreateDate = now,
                        CreateUserId = userId
                    });
                }
            }
        }
    }
}
