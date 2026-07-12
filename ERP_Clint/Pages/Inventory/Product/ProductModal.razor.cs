using ERP_Clint.Service.InventoryService;
using ERPDto.CategoriesDto;
using ERPDto.ProductsDto;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace ERP_Clint.Pages.Inventory.Product
{
    public partial class ProductModal
    {
        [Parameter] public bool IsOpen { get; set; }
        [Inject] private IProductService ProductService { get; set; } = default!;
        [Parameter] public ProductDto? EditingProduct { get; set; }
        [Parameter] public List<CategoryDto>? Categories { get; set; }
        [Parameter] public EventCallback<ProductDto> OnSaved { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Inject] private HttpClient Http { get; set; } = default!;

        private UpdateProductModel formModel = new();
        private CreateProductModel Model = new();
        private bool isSaving;
        private string? errorMessage;
        private bool IsEditMode => EditingProduct is not null;

        protected override void OnParametersSet()
        {
            if (EditingProduct is not null)
            {
                formModel = new UpdateProductModel
                {
                   Id=EditingProduct.Id,
                   Barcode = EditingProduct.Barcode,
                   Name = EditingProduct.Name,
                   SKU = EditingProduct.SKU,
                };
            }
            else
            {
                formModel = new UpdateProductModel();
            }

            errorMessage = null;
        }

        private async Task HandleSave()
        {
            if (IsEditMode&&formModel.CategoryId == 0)
            {
                errorMessage = "يرجى اختيار القسم";
                return;
            }
            isSaving = true;
            errorMessage = null;

            try
            {
                var response = new HttpResponseMessage();
                if (IsEditMode)
                {
                    response = await ProductService.UpdateProduct(formModel);
                    ProductDto dto = new ProductDto
                    {
                        Id= formModel.Id,
                        Barcode = formModel.Barcode,
                        Name = formModel.Name,
                        SKU = formModel.SKU,
                        SellingPrice = formModel.Price
                    };
                    if (response.IsSuccessStatusCode)
                    {
                        if(EditingProduct is not null)
                            await OnSaved.InvokeAsync(dto);
                    } else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        errorMessage = "يوجد منتج بنفس الكود مسبقاً";
                    }
                    else
                    {
                        errorMessage = "حدث خطأ أثناء الحفظ، حاول مرة أخرى";
                    }
                }
                else
                {
                    response = await ProductService.CreateProduct(Model);
                    ProductDto dto = new ProductDto
                    {
                        Barcode = Model.Barcode,
                        Name = Model.Name,
                        SKU = Model.SKU,
                        CostPrice = Model.CostPrice,
                        SellingPrice = Model.SellingPrice,
                        CurrentStock = Model.CurrentStock,
                        MinStockLevel = Model.MinStockLevel,
                    };
                    if (response.IsSuccessStatusCode)
                    {
                        if (EditingProduct is  null)
                            await OnSaved.InvokeAsync(dto);

                    }else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        errorMessage = "يوجد منتج بنفس الكود مسبقاً";
                    }
                    else
                    {
                        errorMessage = "حدث خطأ أثناء الحفظ، حاول مرة أخرى";
                    }
                }
            }
            catch
            {
                errorMessage = "تعذر الاتصال بالخادم";
            }
            finally
            {
                isSaving = false;
            }
        }

        private async Task Close()
        {
            if (isSaving) return;
            await OnClose.InvokeAsync();
        }

    }
}
