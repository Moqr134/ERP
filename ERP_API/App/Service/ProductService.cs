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
            Product? product = await _context.Products.FindAsync(id);
            return product;
        }
        private async Task<Product?> GetProductByName(string name)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Name == name);
            return product;
        }
        private async Task<Product?> GetProductBySKU(string SKU)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(x=>x.SKU == SKU);
            return product;
        }
        private async Task<Product?> GetProductByBarcode(string Barcode)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(x=>x.Barcode == Barcode);
            return product;
        }

        public async Task CreateProduct(CreateProductModel product, int userId)
        {
            Categories? categories = await _context.Categories.FindAsync(userId);
            if (categories == null) 
                throw new KeyNotFoundException("الفئة غير موجوده");
            Product? Newproduct = await GetProductBySKU(product.SKU);
            if (Newproduct != null) throw new DuplicateException("هذا الSKU مستخدم بلفعل في منتج اخر");
            Newproduct = await GetProductByBarcode(product.Barcode);
            if (Newproduct != null) throw new DuplicateException("هذا باركود مستخدم بلفعل في منتج اخر");
            Newproduct = await GetProductByName(product.Name);
            if (product.CostPrice > product.SellingPrice) throw new LogicException("سعر  البيع لا يمكن ان يكون اقل من سعر الكلفة");
            if (product.MinStockLevel < 0) throw new LogicException("اقل قيمه مخزونه لا يمن ان تكون في السالب");
            Newproduct = new Product();
            Newproduct = _mapper.Map<Product>(product);
            Newproduct.CurrentStock = 0;
            Newproduct.CreateDate = DateTime.UtcNow.AddHours(3);
            Newproduct.CreateUserId = userId;
        }

        public async Task DeleteProduct(int id, int userId)
        {
            Product? product = await GetFullProduct(id);
            if (product == null) throw new KeyNotFoundException("حدث خطا اثناء جلب البيانات");
            product.RemoveDate = DateTime.UtcNow.AddHours(3);
            product.RemoveUserId = userId;
            _context.Products.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductDto>> GetAllProductsAsync(PageDto pageDto)
        {
            List<ProductDto> products = await _context.Products
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
                }).ToListAsync();
            if (products == null) throw new KeyNotFoundException("المنتج غير موجود");
            return products;
        }
        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            ProductDto? product = await _context.Products.Where(p => p.Id == id)
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
            if(product.SKU != product.SKU && product.SKU != null)
            {
                Product? SKU = await GetProductBySKU(product.SKU);
                if (SKU != null) throw new DuplicateException("هذا ال SKU مستخدم في منتج اخر");
                UpdateProduct.SKU = product.SKU;
            }
            if(product.Barcode != product.Barcode && product.Barcode != null)
            {
                Product? Barcode = await GetProductByBarcode(product.Barcode);
                if (Barcode != null) throw new DuplicateException("هذا الباركود مستخدم في منتج اخر");
                UpdateProduct.Barcode = product.Barcode;
            }
            if(product.Name != product.Name && product.Name != null)
            {
                Product? Name = await GetProductByName(product.Name);
                if (Name != null) throw new DuplicateException("هذا الاسم مستخدم في منتج اخر");
                UpdateProduct.Name = product.Name;
            }
            UpdateProduct.UpdateDate = DateTime.UtcNow.AddHours(3);
            UpdateProduct.UpdateUserId = userId;
            _context.Products.Entry(UpdateProduct).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
