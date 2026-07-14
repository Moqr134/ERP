using ERP_Clint.Service.InventoryService;
using ERPDto.Suppliers;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace ERP_Clint.Pages.Inventory.Supplier
{
    public partial class Suppliers
    {
        [Inject]
        private ISuppliersService _suppliersService { get; set; } = default!;

        private List<SuppliersDto> suppliers = new();
        private string searchTerm = string.Empty;
        private bool isLoading = true;
        private string? loadError;

        private bool isModalOpen;
        private SuppliersDto? supplierBeingEdited;

        private bool isDeleteModalOpen;
        private SuppliersDto? supplierBeingDeleted;

        private List<SuppliersDto> FilteredSuppliers =>
            string.IsNullOrWhiteSpace(searchTerm)
                ? suppliers
                : suppliers.Where(c => (c.CompanyName ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

        protected override async Task OnInitializedAsync()
        {
            await LoadSuppliers();
        }

        private async Task LoadSuppliers()
        {
            isLoading = true;
            loadError = null;

            try
            {
                var result = await _suppliersService.GetAllSuppliersAsync();
                suppliers = result ?? new List<SuppliersDto>();
            }
            catch
            {
                loadError = "تعذر تحميل الموردين، تأكد من اتصالك وحاول مرة أخرى";
            }
            finally
            {
                isLoading = false;
            }
        }

        private void OpenAddModal()
        {
            supplierBeingEdited = null;
            isModalOpen = true;
        }

        private void OpenEditModal(SuppliersDto supplier)
        {
            supplierBeingEdited = supplier;
            isModalOpen = true;
        }

        private void CloseModal()
        {
            isModalOpen = false;
            supplierBeingEdited = null;
        }

        private async Task HandleSaved(SuppliersDto saved)
        {
            var existingIndex = suppliers.FindIndex(c => c.Id == saved.Id);
            if (existingIndex >= 0)
            {
                suppliers[existingIndex] = saved;
            }
            else
            {
                suppliers.Insert(0, saved);
            }
            await LoadSuppliers();
            isModalOpen = false;
            supplierBeingEdited = null;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private void OpenDeleteModal(SuppliersDto supplier)
        {
            supplierBeingDeleted = supplier;
            isDeleteModalOpen = true;
        }

        private void CloseDeleteModal()
        {
            isDeleteModalOpen = false;
            supplierBeingDeleted = null;
        }

        private async Task HandleDeleteConfirmed()
        {
            if (supplierBeingDeleted is null) return;
            try
            {
                var response = await _suppliersService.DeleteSupplierAsync(supplierBeingDeleted.Id);
                if (response.IsSuccessStatusCode)
                {
                    // refresh list to ensure UI is up to date
                    await LoadSuppliers();
                }
                else
                {
                    loadError = "حدث خطا ما";
                }
            }
            catch (Exception ex)
            {
                loadError = "تعذر حذف المورد: " + ex.Message;
            }
            finally
            {
                isDeleteModalOpen = false;
                supplierBeingDeleted = null;
            }
        }
    }
}
