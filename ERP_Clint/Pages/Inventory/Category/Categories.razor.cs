using ERP_Clint.Service.InventoryService;
using ERPDto.CategoriesDto;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace ERP_Clint.Pages.Inventory.Category
{
    public partial class Categories
    {
        [Inject]
        private ICatigoryService _catigoryService {  get; set; } = default!;
        private List<CategoryDto> categories = new();
        private string searchTerm = string.Empty;
        private bool isLoading = true;
        private string? loadError;

        private bool isModalOpen;
        private CategoryDto? categoryBeingEdited;

        private bool isDeleteModalOpen;
        private CategoryDto? categoryBeingDeleted;

        private List<CategoryDto> FilteredCategories =>
            string.IsNullOrWhiteSpace(searchTerm)
                ? categories
                : categories.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

        protected override async Task OnInitializedAsync()
        {
            await LoadCategories();
        }

        private async Task LoadCategories()
        {
            isLoading = true;
            loadError = null;

            try
            {
                var result = await _catigoryService.GetAllCategoriesAsync();
                categories = result ?? new List<CategoryDto>();
            }
            catch
            {
                loadError = "تعذر تحميل الأقسام، تأكد من اتصالك وحاول مرة أخرى";
            }
            finally
            {
                isLoading = false;
            }
        }

        private void OpenAddModal()
        {
            categoryBeingEdited = null;
            isModalOpen = true;
        }

        private void OpenEditModal(CategoryDto category)
        {
            categoryBeingEdited = category;
            isModalOpen = true;
        }

        private void CloseModal()
        {
            isModalOpen = false;
            categoryBeingEdited = null;
        }

        private async Task HandleSaved(CategoryDto saved)
        {
            var existingIndex = categories.FindIndex(c => c.Id == saved.Id);
            if (existingIndex >= 0)
            {
                categories[existingIndex] = saved; 
            }
            else
            {
                categories.Insert(0, saved); 
            }
            await LoadCategories();
            isModalOpen = false;
            categoryBeingEdited = null;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private void OpenDeleteModal(CategoryDto category)
        {
            categoryBeingDeleted = category;
            isDeleteModalOpen = true;
        }

        private void CloseDeleteModal()
        {
            isDeleteModalOpen = false;
            categoryBeingDeleted = null;
        }

        private async Task HandleDeleteConfirmed()
        {
            if (categoryBeingDeleted is null) return;
            if (categoryBeingDeleted.ProductCount > 0)
            {
                loadError = "لا يمكن حذف الفئة لأنها تحتوي على منتجات مرتبطة بها.";
                isDeleteModalOpen = false;
                return;
            }
            try
            {
                var response = await _catigoryService.DeleteCategoryAsync(categoryBeingDeleted.Id);

                if (response.IsSuccessStatusCode)
                {
                    categories.RemoveAll(c => c.Id == categoryBeingDeleted.Id);
                }
                else
                {
                    loadError = "حدث خطا ما";
                }
            }
            finally
            {
                isDeleteModalOpen = false;
                categoryBeingDeleted = null;
            }
        }
    }
}
