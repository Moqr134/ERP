using ERP_API.Domin.CategoriesEntity;
using ERPDto.CategoriesDto;

namespace ERP_API.App.IService
{
    public interface ICategoriesService
    {
        public Task<List<CategoryDto>> GetAllCategories();
        public Task<CategoryDto> GetCategoryById(int id);
        public Task<Categories?> GetFullCategoryById(int id);
        public Task CreateCategory(CategoryDto categories, int userId);
        public Task UpdateCategory(CategoryDto categories, int UpdateUserId);
        public Task DeleteCategory(int Categoryid, int DeleteUserId);
    }
}
