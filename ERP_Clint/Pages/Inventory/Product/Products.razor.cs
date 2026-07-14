using ERP_Clint.Service.InventoryService;
using ERPDto.CategoriesDto;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace ERP_Clint.Pages.Inventory.Product
{
    public partial class Products
    {

        private List<ProductDto> products = new();
        private List<CategoryDto> categories = new();
        [Inject]
        private IProductService _productService {  get; set; } = default!;
        [Inject] private ICatigoryService _catigoryService { get; set; } = default!;
        private PageDto page = new PageDto();
        private string searchTerm = string.Empty;
        private ProductsInfo? productsInfo;
        private int categoryFilter = 0;
        private string categorySearch = string.Empty;
        private bool isCategoryDropdownOpen;

        private List<CategoryDto> FilteredCategories =>
            string.IsNullOrWhiteSpace(categorySearch)
                ? categories
                : categories.Where(c => (c.Name ?? string.Empty).Contains(categorySearch, StringComparison.OrdinalIgnoreCase)).ToList();

        private bool isLoading = true;
        private string? loadError;

        private bool isModalOpen;
        private ProductDto? productBeingEdited;

        private bool isDeleteModalOpen;
        private ProductDto? productBeingDeleted;

        private List<ProductDto> FilteredProducts =>
            products
                .Where(p => (categoryFilter == 0 || p.CategoriesId == categoryFilter)
                    && (string.IsNullOrWhiteSpace(searchTerm)
                        || p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                        || p.Barcode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();

        protected override async Task OnInitializedAsync()
        {
            await LoadAll();
        }

        private async Task LoadAll()
        {
            isLoading = true;
            loadError = null;

            try
            {
                var requst = _catigoryService.GetAllCategoriesAsync();
                var productsRequest = _productService.GetAllProductsAsync(page);
                var productsInfoRequest = _productService.GetProductsInfo();
                await Task.WhenAll(requst, productsRequest, productsInfoRequest);
                categories = await requst ?? new List<CategoryDto>();
                
                if(categories.Count == 0 && loadError is null) {
                    // قائمة فارغة مقبولة
                }
                products = await productsRequest ?? new List<ProductDto>();
                productsInfo = await productsInfoRequest;
                if(productsInfo is null)
                {
                    loadError = "تعذر تحميل بيانات معلومات المنتجات، تأكد من اتصالك وحاول مرة أخرى";
                }
            }
            finally
            {
                isLoading = false;
            }
        }
        private int CurrentTotalPages => productsInfo?.PageCount ?? 0;
        private async Task GoToPage(int pageNumper)
        {
            if (pageNumper < 1 || pageNumper > CurrentTotalPages || pageNumper == page.PageIndex) return;
            page.PageIndex = pageNumper;
            await LoadAll();
        }
        private List<int> GetVisiblePageNumbers()
        {
            var totalPages = CurrentTotalPages;
            if (totalPages == 0) return new List<int>();

            const int windowSize = 5;
            var start = Math.Max(1, page.PageIndex - windowSize / 2);
            var end = Math.Min(totalPages, start + windowSize - 1);
            start = Math.Max(1, end - windowSize + 1);

            return Enumerable.Range(start, Math.Max(0, end - start + 1)).ToList();
        }
        private void OpenAddModal()
        {
            productBeingEdited = null;
            categories = categories ?? new List<CategoryDto>();
            isModalOpen = true;
        }

        private void OpenCategoryDropdown()
        {
            isCategoryDropdownOpen = true;
        }

        private void SelectCategory(CategoryDto? category)
        {
            if (category == null || category.Id == 0)
            {
                categoryFilter = 0;
                categorySearch = string.Empty;
            }
            else
            {
                categoryFilter = category.Id;
                categorySearch = category.Name ?? string.Empty;
            }
            isCategoryDropdownOpen = false;
        }

        private void OpenEditModal(ProductDto product)
        {
            productBeingEdited = product;
            categories = categories ?? new List<CategoryDto>();
            isModalOpen = true;
        }

        private void CloseModal()
        {
            isModalOpen = false;
            productBeingEdited = null;
        }

        private async Task HandleSaved(ProductDto saved)
        {
            if (productsInfo is null) return;
            productsInfo.TotalProducts++;
            if(productsInfo.TotalProducts % page.PageSize == 1 && products.Count == page.PageSize)
            {
                products.RemoveAt(products.Count - 1);
            }
            if(saved.CurrentStock < saved.MinStockLevel)
                productsInfo.ProductsCountLissMinStock++;
            if(saved.CurrentStock == 0)
                productsInfo.ProductsStockOut++;
            products.Insert(0, saved);
            await LoadAll();
            isModalOpen = false;
            productBeingEdited = null;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private void OpenDeleteModal(ProductDto product)
        {
            productBeingDeleted = product;
            isDeleteModalOpen = true;
        }

        private void CloseDeleteModal()
        {
            isDeleteModalOpen = false;
            productBeingDeleted = null;
        }

        private async Task HandleDeleteConfirmed()
        {
            if (productBeingDeleted is null || productsInfo is null) return;

            try
            {
                var response = await _productService.DeleteProduct(productBeingDeleted.Id);
                if (response.IsSuccessStatusCode)
                {
                    var newTotalCount = Math.Max(0, productsInfo.TotalProducts - 1);
                    var newTotalPages = (int)Math.Ceiling((double)newTotalCount / page.PageSize);

                    products.RemoveAll(p => p.Id == productBeingDeleted.Id);

                    if (products.Count == 0)
                    {
                        // القائمة فضت بعد الحذف: لو كنا بصفحة غير الأولى نرجع صفحة للخلف ونعيد التحميل كامل
                        if (page.PageIndex > 1)
                        {
                            page.PageIndex--;
                            await LoadAll();
                        }
                        // إذا كنا أصلاً بالصفحة 1 وفضت، نتركها فاضية (تظهر رسالة "لا توجد منتجات")
                    }
                    else
                    {
                        // نحدث العداد الكلي على العناصر المتبقية بدون الحاجة لإعادة تحميل من الـ API
                        foreach (var product in products)
                        {
                            productsInfo.TotalProducts = newTotalCount;
                            productsInfo.PageCount = newTotalPages;
                        }
                    }
                }
            }
            finally
            {
                isDeleteModalOpen = false;
                productBeingDeleted = null;
            }
        }

        private static string FormatCurrency(decimal amount) => $"{amount:N0} د.ع";
    }
}
