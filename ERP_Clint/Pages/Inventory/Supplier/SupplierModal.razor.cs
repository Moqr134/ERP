using ERP_Clint.Service.InventoryService;
using ERPDto.Suppliers;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace ERP_Clint.Pages.Inventory.Supplier
{
    public partial class SupplierModal
    {
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public SuppliersDto? EditingSupplier { get; set; }
        [Parameter] public EventCallback<SuppliersDto> OnSaved { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        [Inject] private ISuppliersService suppliersService { get; set; } = default!;

        private SuppliersDto formModel = new();
        private bool isSaving;
        private string? errorMessage;

        private bool IsEditMode => EditingSupplier is not null;

        protected override void OnParametersSet()
        {
            if (EditingSupplier is not null)
            {
                formModel = new SuppliersDto
                {
                    Id = EditingSupplier.Id,
                    CompanyName = EditingSupplier.CompanyName,
                    ContactName = EditingSupplier.ContactName,
                    PhoneNumper = EditingSupplier.PhoneNumper,
                };
            }
            else
            {
                formModel = new SuppliersDto();
            }

            errorMessage = null;
        }

        private async Task HandleSave()
        {
            isSaving = true;
            errorMessage = null;

            try
            {
                HttpResponseMessage response;

                if (IsEditMode)
                {
                    response = await suppliersService.UpdateSupplierAsync(formModel);
                }
                else
                {
                    response = await suppliersService.CreateSupplierAsync(formModel);
                }
                if (response.IsSuccessStatusCode)
                {
                    await OnSaved.InvokeAsync(formModel);
                    await OnClose.InvokeAsync();
                    return;
                }
                else
                {
                    errorMessage = "حدث خطأ أثناء الحفظ، حاول مرة أخرى";
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
