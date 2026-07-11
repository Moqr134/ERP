using ERP_Clint.Service.InventoryService;
using ERPDto.CategoriesDto;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace ERP_Clint.Pages.Inventory.Category
{
    public partial class CategoryModal
    {
        /// <summary>هل الـ Modal مفتوح حالياً</summary>
        [Parameter] public bool IsOpen { get; set; }

        /// <summary>القسم المراد تعديله. اتركه null عند الإضافة</summary>
        [Parameter] public CategoryDto? EditingCategory { get; set; }

        /// <summary>يستدعى بعد نجاح الحفظ (إضافة أو تعديل)، يمرر القسم المحدّث</summary>
        [Parameter] public EventCallback<CategoryDto> OnSaved { get; set; }

        /// <summary>يستدعى عند الإلغاء أو إغلاق الـ Modal</summary>
        [Parameter] public EventCallback OnClose { get; set; }

        [Inject] private HttpClient Http { get; set; } = default!;

        private CategoryDto formModel = new();
        private bool isSaving;
        private string? errorMessage;
        [Inject]
        private ICatigoryService catigoryService { get; set; } = default!;
        private bool IsEditMode => EditingCategory is not null;

        protected override void OnParametersSet()
        {
            if (EditingCategory is not null)
            {
                formModel = new CategoryDto
                {
                    Id = EditingCategory.Id,
                    Name = EditingCategory.Name,
                    Description = EditingCategory.Description,
                    ProductCount = EditingCategory.ProductCount,
                };
            }
            else
            {
                formModel = new CategoryDto();
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
                    response = await catigoryService.UpdateCategoryAsync(formModel);
                }
                else
                {
                    response = await catigoryService.CreateCategoryAsync(formModel);
                }
                if (response.IsSuccessStatusCode)
                {
                    await OnSaved.InvokeAsync(formModel);
                    await OnClose.InvokeAsync();
                    return;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    errorMessage = "يوجد قسم بنفس الاسم مسبقاً";
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
