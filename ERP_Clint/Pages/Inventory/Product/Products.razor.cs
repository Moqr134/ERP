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

        private List<ProductDto>? products = new();
        private List<CategoryDto>? categories = new();
        [Inject]
        private IProductService _productService {  get; set; } = default!;
        [Inject] private ICatigoryService _catigoryService { get; set; } = default!;
        private PageDto page = new PageDto();
        private string searchTerm = string.Empty;
        private ProductsInfo? productsInfo;
        private int categoryFilter = 0;

        private bool isLoading = true;
        private string? loadError;

        private bool isModalOpen;
        private ProductDto? productBeingEdited;

        private bool isDeleteModalOpen;
        private ProductDto? productBeingDeleted;

        private List<ProductDto> FilteredProducts =>
            products
                .Where(p => string.IsNullOrWhiteSpace(searchTerm)
                    || p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    || p.Barcode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
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
                categories = await requst;
                
                if(categories == null) {
                    loadError = "تعذر تحميل بيانات الفئات، تأكد من اتصالك وحاول مرة أخرى";
                }
                products = await productsRequest;
                if (products is null)
                {
                    loadError = "تعذر تحميل بيانات المنتجات، تأكد من اتصالك وحاول مرة أخرى";
                }
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
        private async Task GoToPage(int pageNumper)
        {
            if (pageNumper < 1 || pageNumper > products[0].totalPages || pageNumper == page.PageIndex) return;
            page.PageIndex = pageNumper;
            await LoadAll();
        }
        private List<int> GetVisiblePageNumbers()
        {
            const int windowSize = 5;
            var start = Math.Max(1, page.PageIndex - windowSize / 2);
            var end = Math.Min(products[0].totalPages, start + windowSize - 1);
            start = Math.Max(1, end - windowSize + 1);

            return Enumerable.Range(start, Math.Max(0, end - start + 1)).ToList();
        }
        private void OpenAddModal()
        {
            productBeingEdited = null;
            categories = categories ?? new List<CategoryDto>();
            isModalOpen = true;
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
            var index = products.FindIndex(p => p.Id == saved.Id);
            if (index >= 0)
            {
                products[index] = saved;
            }
            else
            {
                var count = products[0].totalCount++;
                products.Insert(0, saved);
                products[0].totalCount = count;
                products[0].totalPages = (int)Math.Ceiling((double)products[0].totalCount / page.PageSize);
            }
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
            if (productBeingDeleted is null) return;

            try
            {
                var response = await _productService.DeleteProduct(productBeingDeleted.Id);
                if (response.IsSuccessStatusCode)
                {
                    products.RemoveAll(p => p.Id == productBeingDeleted.Id);
                    products[0] = new ProductDto
                    {
                        totalCount = products[0].totalCount - 1,
                        totalPages = (int)Math.Ceiling((double)(products[0].totalCount - 1) / page.PageSize)
                    };
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
