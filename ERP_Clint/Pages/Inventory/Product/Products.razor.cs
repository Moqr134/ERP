using ERP_Clint.Service;
using ERP_Clint.Service.InventoryService;
using ERPDto.CategoriesDto;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using ERPDto.WarehouseDto;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace ERP_Clint.Pages.Inventory.Product
{
    public partial class Products
    {

        private List<ProductDto> products = new();
        private List<CategoryDto> categories = new();
        private List<WarehouseDto> warehouses = new();
        [Inject]
        private IProductService _productService {  get; set; } = default!;
        [Inject] private ICatigoryService _catigoryService { get; set; } = default!;
        [Inject] private IWarehousesService _warehousesService { get; set; } = default!;
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
        private string? deleteError;

        private CancellationTokenSource? _searchCts;

        protected override async Task OnInitializedAsync()
        {
            await LoadAll();
        }

        private void ApplyFiltersToPage()
        {
            page.SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
            page.CategoryId = categoryFilter;
        }

        private async Task LoadAll()
        {
            isLoading = true;
            loadError = null;
            ApplyFiltersToPage();

            try
            {
                var requst = _catigoryService.GetAllCategoriesAsync();
                var productsRequest = _productService.GetAllProductsAsync(page);
                var productsInfoRequest = _productService.GetProductsInfo(page);
                var warehousesRequest = LoadWarehousesSafeAsync();
                await Task.WhenAll(requst, productsRequest, productsInfoRequest, warehousesRequest);
                categories = await requst ?? new List<CategoryDto>();
                products = await productsRequest ?? new List<ProductDto>();
                productsInfo = await productsInfoRequest;
                warehouses = await warehousesRequest;
                if(productsInfo is null)
                {
                    loadError = "تعذر تحميل بيانات معلومات المنتجات، تأكد من اتصالك وحاول مرة أخرى";
                }
            }
            catch (ApiRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                loadError = "انتهت الجلسة، يرجى تسجيل الدخول مرة أخرى";
            }
            catch (Exception)
            {
                loadError = "تعذر تحميل المنتجات، تأكد من اتصالك وحاول مرة أخرى";
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task<List<WarehouseDto>> LoadWarehousesSafeAsync()
        {
            try
            {
                return await _warehousesService.GetAllWarehousesAsync();
            }
            catch
            {
                // Warehouse listing is optional for the product form;
                // a user may lack GetAllWarehouses permission.
                return new List<WarehouseDto>();
            }
        }

        private async Task OnSearchChanged(ChangeEventArgs e)
        {
            searchTerm = e.Value?.ToString() ?? string.Empty;
            page.PageIndex = 1;
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;
            try
            {
                await Task.Delay(350, token);
                if (!token.IsCancellationRequested)
                    await LoadAll();
            }
            catch (TaskCanceledException) { }
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

        private async Task SelectCategory(CategoryDto? category)
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
            page.PageIndex = 1;
            await LoadAll();
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
            await LoadAll();
            isModalOpen = false;
            productBeingEdited = null;
        }

        private void OpenDeleteModal(ProductDto product)
        {
            productBeingDeleted = product;
            deleteError = null;
            isDeleteModalOpen = true;
        }

        private void CloseDeleteModal()
        {
            isDeleteModalOpen = false;
            productBeingDeleted = null;
            deleteError = null;
        }

        private async Task HandleDeleteConfirmed()
        {
            if (productBeingDeleted is null) return;
            deleteError = null;

            try
            {
                var response = await _productService.DeleteProduct(productBeingDeleted.Id);
                if (response.IsSuccessStatusCode)
                {
                    isDeleteModalOpen = false;
                    productBeingDeleted = null;
                    await LoadAll();
                }
                else
                {
                    deleteError = "تعذر حذف المنتج، حاول مرة أخرى";
                }
            }
            catch
            {
                deleteError = "تعذر الاتصال بالخادم أثناء الحذف";
            }
        }

        private static string FormatCurrency(decimal amount) => $"{amount:N0} د.ع";
    }
}
