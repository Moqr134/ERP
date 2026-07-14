using ERP_Clint.Service.InventoryService;
using ERPDto.ProductsDto;
using ERPDto.StockTransactionDto;
using Microsoft.AspNetCore.Components;

namespace ERP_Clint.Pages.Inventory.Stock
{
    public partial class StockTransactionModal
    {
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnSuccess { get; set; }

        [Inject] private IStockTransactionsService StockTransactionsService { get; set; } = default!;
        [Inject] private IProductService ProductService { get; set; } = default!;

        private CreateStockTransactionsModel formModel = new() { Quantity = 1, TransactionType = "In" };
        private bool isSaving;
        private string? errorMessage;
        private List<ProductDto> Products = new();
        private string productSearch = string.Empty;
        private bool isDropdownOpen;

        private List<ProductDto> FilteredProducts =>
            string.IsNullOrWhiteSpace(productSearch)
                ? Products
                : Products.Where(p => (p.Name ?? string.Empty).Contains(productSearch, StringComparison.OrdinalIgnoreCase)
                                  || (p.Barcode ?? string.Empty).Contains(productSearch, StringComparison.OrdinalIgnoreCase)).ToList();

        private bool IsEditMode => false;

        protected override async Task OnParametersSetAsync()
        {
            errorMessage = null;
            // load products for select (first page with large size)
            try
            {
                var list = await ProductService.GetAllProductsAsync(new ERPDto.PaigingDto.PageDto { PageIndex = 1, PageSize = 200 });
                Products = list ?? new List<ProductDto>();
            }
            catch
            {
                Products = new List<ProductDto>();
            }
        }

        private async Task HandleSave()
        {
            if (formModel.ProductId <= 0)
            {
                errorMessage = "يرجى اختيار منتج";
                return;
            }
            if (formModel.Quantity <= 0)
            {
                errorMessage = "الكمية يجب أن تكون أكبر من صفر";
                return;
            }

            isSaving = true;
            errorMessage = null;
            try
            {
                var response = await StockTransactionsService.AddStockTransaction(formModel);
                if (response.IsSuccessStatusCode)
                {
                    await OnSuccess.InvokeAsync();
                    await Close();
                    return;
                }

                var raw = await response.Content.ReadAsStringAsync();
                errorMessage = TryParseApiMessage(raw) ?? "فشلت العملية، حاول مرة أخرى";
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isSaving = false;
            }
        }

        private static string? TryParseApiMessage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("Message", out var msg))
                    return msg.GetString();
                if (doc.RootElement.TryGetProperty("message", out var msg2))
                    return msg2.GetString();
            }
            catch { }
            return raw.Length > 200 ? null : raw.Trim('"');
        }

        private async Task Close()
        {
            if (isSaving) return;
            // reset form
            formModel = new CreateStockTransactionsModel { Quantity = 1, TransactionType = "In" };
            productSearch = string.Empty;
            isDropdownOpen = false;
            await OnClose.InvokeAsync();
        }

        private void OpenDropdown()
        {
            isDropdownOpen = true;
        }

        private void SelectProduct(ProductDto p)
        {
            formModel.ProductId = p.Id;
            productSearch = p.Name ?? string.Empty;
            isDropdownOpen = false;
        }
    }
}
