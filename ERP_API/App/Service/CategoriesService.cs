using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.CategoriesEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.CategoriesDto;
using Infrastructure.AppException;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class CategoriesService : MasterService, ICategoriesService, IScopped
    {
        public CategoriesService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }
        public async Task<Categories?> GetFullCategoryById(int id)
        {
            Categories? categories = await _context.Categories
                .Include(x => x.Products.Where(p => !p.IsRemoved))
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsRemoved);
            return categories;
        }
        public async Task CreateCategory(CategoryDto categories, int userId)
        {
            Categories? category = await GetFullCategoryByName(categories.Name);
            if (category != null)
            {
                throw new DuplicateException("الفئة موجودة بالفعل");
            }
            category = new Categories
            {
                Name = categories.Name,
                Description = categories.Description,
                CreateDate = DateTime.UtcNow.AddHours(3),
                CreateUserId = userId
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategory(int Categoryid, int DeleteUserId)
        {
            Categories? category = await GetFullCategoryById(Categoryid);
            if (category == null)
            {
                throw new KeyNotFoundException("لم يتم العثور على الفئة");
            }
            if(category.Products.Count > 0)
            {
                throw new InvalidOperationException("لا يمكن حذف الفئة لأنها تحتوي على منتجات مرتبطة بها.");
            }
            category.IsRemoved = true;
            category.RemoveDate = DateTime.UtcNow.AddHours(3);
            category.RemoveUserId = DeleteUserId;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CategoryDto>> GetAllCategories()
        {
            List<CategoryDto> categoryDtos = await _context.Categories
                .Where(c => !c.IsRemoved)
                .Include(p => p.Products.Where(x => !x.IsRemoved))
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count
                }).ToListAsync();
            return categoryDtos;
        }

        public async Task<CategoryDto> GetCategoryById(int id)
        {
            CategoryDto? categories = await _context.Categories.Where(c => c.Id == id && !c.IsRemoved)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).FirstOrDefaultAsync();
            if (categories == null)
            {
                throw new KeyNotFoundException("لم يتم العثور على الفئة");
            }
            return categories;
        }
        
        public async Task<Categories?> GetFullCategoryByName(string name)
        {
            Categories? categories = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name && !c.IsRemoved);
            return categories;
        }

        public async Task UpdateCategory(CategoryDto categories, int UpdateUserId)
        {
            Categories? category = await GetFullCategoryById(categories.Id);
            if (category == null)
            {
                throw new KeyNotFoundException("لم يتم العثور على الفئة");
            }
            category.Name = categories.Name;
            category.Description = categories.Description;
            category.UpdateDate = DateTime.UtcNow.AddHours(3);
            category.UpdateUserId = UpdateUserId;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}