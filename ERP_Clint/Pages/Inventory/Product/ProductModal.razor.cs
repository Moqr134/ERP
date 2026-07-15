using ERP_Clint.Service.InventoryService;
using ERPDto.CategoriesDto;
using ERPDto.ProductsDto;
using ERPDto.WarehouseDto;
using Microsoft.AspNetCore.Components;

namespace ERP_Clint.Pages.Inventory.Product
{
    public partial class ProductModal
    {
        [Parameter] public bool IsOpen { get; set; }
        [Inject] private IProductService ProductService { get; set; } = default!;
        [Parameter] public ProductDto? EditingProduct { get; set; }
        [Parameter] public List<CategoryDto>? Categories { get; set; }
        [Parameter] public List<WarehouseDto>? Warehouses { get; set; }
        [Parameter] public EventCallback<ProductDto> OnSaved { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private IEnumerable<WarehouseDto> ActiveWarehouses =>
            (Warehouses ?? new List<WarehouseDto>())
                .Where(w => w.IsActive || w.Id == (IsEditMode ? formModel.WarehouseId : Model.WarehouseId));

        private UpdateProductModel formModel = new();
        private CreateProductModel Model = new();
        private List<ProductUnitInputDto> unitDrafts = new();
        private bool isSaving;
        private string? errorMessage;
        private int? lastEditingId;
        private bool draftLoaded;
        private bool IsEditMode => EditingProduct is not null;

        protected override void OnParametersSet()
        {
            if (!IsOpen)
            {
                draftLoaded = false;
                lastEditingId = null;
                return;
            }

            var editingId = EditingProduct?.Id;
            if (draftLoaded && lastEditingId == editingId)
                return;

            if (EditingProduct is not null)
            {
                formModel = new UpdateProductModel
                {
                    Id = EditingProduct.Id,
                    Barcode = EditingProduct.Barcode,
                    Name = EditingProduct.Name,
                    SKU = EditingProduct.SKU,
                    Price = EditingProduct.SellingPrice,
                    CostPrice = EditingProduct.CostPrice,
                    MinStockLevel = EditingProduct.MinStockLevel,
                    CategoryId = EditingProduct.CategoriesId,
                    WarehouseId = EditingProduct.WarehouseId,
                };
                unitDrafts = MapUnitsFromDto(EditingProduct.Units);
                if (unitDrafts.Count == 0)
                    unitDrafts = SeedDefaultUnit(EditingProduct.Barcode, EditingProduct.SellingPrice);
            }
            else
            {
                formModel = new UpdateProductModel();
                Model = new CreateProductModel();
                unitDrafts = SeedDefaultUnit(string.Empty, 0);
            }

            lastEditingId = editingId;
            draftLoaded = true;
            errorMessage = null;
        }

        private static List<ProductUnitInputDto> SeedDefaultUnit(string barcode, double price) =>
        [
            new ProductUnitInputDto
            {
                Name = "مفرد",
                Factor = 1,
                SellingPrice = price,
                IsBase = true,
                IsDefaultForSale = true,
                SortOrder = 0,
                Barcodes =
                [
                    new ProductBarcodeInputDto
                    {
                        Barcode = barcode ?? string.Empty,
                        IsPrimary = true
                    }
                ]
            }
        ];

        private static List<ProductUnitInputDto> MapUnitsFromDto(List<ProductUnitDto>? units)
        {
            if (units is null || units.Count == 0)
                return new();

            return units
                .OrderBy(u => u.SortOrder)
                .ThenBy(u => u.Id)
                .Select(u => new ProductUnitInputDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Factor = u.Factor,
                    SellingPrice = u.SellingPrice,
                    IsBase = u.IsBase,
                    IsDefaultForSale = u.IsDefaultForSale,
                    SortOrder = u.SortOrder,
                    Barcodes = u.Barcodes
                        .Select(b => new ProductBarcodeInputDto
                        {
                            Id = b.Id,
                            Barcode = b.Barcode,
                            IsPrimary = b.IsPrimary
                        })
                        .ToList()
                })
                .ToList();
        }

        private void AddUnit(string? presetName = null)
        {
            var name = presetName ?? $"وحدة {unitDrafts.Count + 1}";
            var factor = presetName switch
            {
                "كارتون" => 24,
                "باكيت" => 10,
                "علبة" => 12,
                _ => Math.Max(1, unitDrafts.Count == 0 ? 1 : 2)
            };
            var isFirst = unitDrafts.Count == 0;

            unitDrafts.Add(new ProductUnitInputDto
            {
                Name = name,
                Factor = isFirst ? 1 : factor,
                SellingPrice = isFirst
                    ? (IsEditMode ? formModel.Price : Model.SellingPrice)
                    : 0,
                IsBase = isFirst,
                IsDefaultForSale = isFirst,
                SortOrder = unitDrafts.Count,
                Barcodes =
                [
                    new ProductBarcodeInputDto { IsPrimary = true }
                ]
            });
        }

        private void RemoveUnit(ProductUnitInputDto unit)
        {
            if (unitDrafts.Count <= 1)
            {
                errorMessage = "يجب الإبقاء على وحدة واحدة على الأقل";
                return;
            }

            unitDrafts.Remove(unit);
            if (!unitDrafts.Any(u => u.IsBase))
            {
                var baseCandidate = unitDrafts.FirstOrDefault(u => u.Factor == 1) ?? unitDrafts[0];
                baseCandidate.IsBase = true;
                baseCandidate.Factor = 1;
            }

            if (!unitDrafts.Any(u => u.IsDefaultForSale))
                unitDrafts.First(u => u.IsBase).IsDefaultForSale = true;
        }

        private void SetBaseUnit(ProductUnitInputDto unit)
        {
            foreach (var u in unitDrafts)
                u.IsBase = false;
            unit.IsBase = true;
            unit.Factor = 1;
        }

        private void SetDefaultSaleUnit(ProductUnitInputDto unit)
        {
            foreach (var u in unitDrafts)
                u.IsDefaultForSale = false;
            unit.IsDefaultForSale = true;
        }

        private void AddBarcode(ProductUnitInputDto unit)
        {
            unit.Barcodes.Add(new ProductBarcodeInputDto
            {
                IsPrimary = unit.Barcodes.Count == 0
            });
        }

        private void RemoveBarcode(ProductUnitInputDto unit, ProductBarcodeInputDto barcode)
        {
            if (unit.Barcodes.Count <= 1)
            {
                errorMessage = "كل وحدة تحتاج باركود واحد على الأقل";
                return;
            }

            unit.Barcodes.Remove(barcode);
            if (!unit.Barcodes.Any(b => b.IsPrimary))
                unit.Barcodes[0].IsPrimary = true;
        }

        private void SetPrimaryBarcode(ProductUnitInputDto unit, ProductBarcodeInputDto barcode)
        {
            foreach (var b in unit.Barcodes)
                b.IsPrimary = false;
            barcode.IsPrimary = true;
        }

        private string? ValidateUnitsLocally()
        {
            if (unitDrafts.Count == 0)
                return "أضف وحدة بيع واحدة على الأقل";

            if (unitDrafts.Count(u => u.IsBase) != 1)
                return "حدد وحدة أساس واحدة (مفرد) بمعامل 1";

            var baseUnit = unitDrafts.First(u => u.IsBase);
            if (baseUnit.Factor != 1)
                return "وحدة الأساس يجب أن يكون معاملها 1";

            if (unitDrafts.Count(u => u.IsDefaultForSale) != 1)
                return "حدد وحدة بيع افتراضية واحدة فقط";

            foreach (var unit in unitDrafts)
            {
                if (string.IsNullOrWhiteSpace(unit.Name))
                    return "اسم الوحدة مطلوب";
                if (unit.Factor < 1)
                    return $"معامل الوحدة «{unit.Name}» غير صحيح";
                if (unit.SellingPrice < 0)
                    return $"سعر الوحدة «{unit.Name}» غير صحيح";

                var barcodes = unit.Barcodes
                    .Where(b => !string.IsNullOrWhiteSpace(b.Barcode))
                    .ToList();
                if (barcodes.Count == 0)
                    return $"الوحدة «{unit.Name}» تحتاج باركود واحد على الأقل";

                foreach (var b in barcodes)
                {
                    var code = b.Barcode.Trim();
                    if (code.Length < 3 || code.Length > 50)
                        return $"الباركود «{code}» يجب أن يكون بين 3 و 50 حرفاً";
                }
            }

            var nameDup = unitDrafts
                .GroupBy(u => u.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (nameDup is not null)
                return $"تكرار اسم الوحدة: {nameDup.Key}";

            var allCodes = unitDrafts
                .SelectMany(u => u.Barcodes)
                .Where(b => !string.IsNullOrWhiteSpace(b.Barcode))
                .Select(b => b.Barcode.Trim())
                .ToList();
            var codeDup = allCodes
                .GroupBy(c => c, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (codeDup is not null)
                return $"الباركود مكرر داخل المنتج: {codeDup.Key}";

            return null;
        }

        private List<ProductUnitInputDto> BuildUnitsPayload()
        {
            for (var i = 0; i < unitDrafts.Count; i++)
                unitDrafts[i].SortOrder = i;

            return unitDrafts.Select(u => new ProductUnitInputDto
            {
                Id = u.Id,
                Name = u.Name.Trim(),
                Factor = u.Factor,
                SellingPrice = u.SellingPrice,
                IsBase = u.IsBase,
                IsDefaultForSale = u.IsDefaultForSale,
                SortOrder = u.SortOrder,
                Barcodes = u.Barcodes
                    .Where(b => !string.IsNullOrWhiteSpace(b.Barcode))
                    .Select(b => new ProductBarcodeInputDto
                    {
                        Id = b.Id,
                        Barcode = b.Barcode.Trim(),
                        IsPrimary = b.IsPrimary
                    })
                    .ToList()
            }).ToList();
        }

        private void SyncHeaderFromUnits(List<ProductUnitInputDto> units)
        {
            var baseUnit = units.First(u => u.IsBase);
            var primary = baseUnit.Barcodes.FirstOrDefault(b => b.IsPrimary)
                          ?? baseUnit.Barcodes.First();

            if (IsEditMode)
            {
                formModel.Barcode = primary.Barcode;
                formModel.Price = baseUnit.SellingPrice;
                formModel.Units = units;
            }
            else
            {
                Model.Barcode = primary.Barcode;
                Model.SellingPrice = baseUnit.SellingPrice;
                Model.Units = units;
            }
        }

        private async Task HandleSave()
        {
            var selectedCategoryId = IsEditMode ? formModel.CategoryId : Model.CategoriesId;
            if (selectedCategoryId == 0)
            {
                errorMessage = "يرجى اختيار القسم";
                return;
            }

            var unitError = ValidateUnitsLocally();
            if (unitError is not null)
            {
                errorMessage = unitError;
                return;
            }

            var units = BuildUnitsPayload();
            SyncHeaderFromUnits(units);

            isSaving = true;
            errorMessage = null;

            try
            {
                HttpResponseMessage response;
                if (IsEditMode)
                {
                    response = await ProductService.UpdateProduct(formModel);
                    if (response.IsSuccessStatusCode)
                    {
                        var dto = new ProductDto
                        {
                            Id = formModel.Id,
                            Barcode = formModel.Barcode,
                            Name = formModel.Name,
                            SKU = formModel.SKU,
                            SellingPrice = formModel.Price,
                            CostPrice = formModel.CostPrice ?? 0,
                            MinStockLevel = formModel.MinStockLevel ?? 0,
                            CategoriesId = formModel.CategoryId,
                            WarehouseId = formModel.WarehouseId,
                            Units = units.Select(MapInputToUnitDto).ToList()
                        };
                        await OnSaved.InvokeAsync(dto);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        errorMessage = "يوجد منتج أو باركود مكرر مسبقاً";
                    }
                    else
                    {
                        errorMessage = await ReadApiErrorAsync(response)
                                       ?? "حدث خطأ أثناء الحفظ، حاول مرة أخرى";
                    }
                }
                else
                {
                    response = await ProductService.CreateProduct(Model);
                    if (response.IsSuccessStatusCode)
                    {
                        var dto = new ProductDto
                        {
                            Barcode = Model.Barcode,
                            Name = Model.Name,
                            SKU = Model.SKU,
                            CostPrice = Model.CostPrice,
                            SellingPrice = Model.SellingPrice,
                            CurrentStock = Model.CurrentStock,
                            MinStockLevel = Model.MinStockLevel,
                            CategoriesId = Model.CategoriesId,
                            WarehouseId = Model.WarehouseId,
                            Units = units.Select(MapInputToUnitDto).ToList()
                        };
                        await OnSaved.InvokeAsync(dto);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        errorMessage = "يوجد منتج أو باركود مكرر مسبقاً";
                    }
                    else
                    {
                        errorMessage = await ReadApiErrorAsync(response)
                                       ?? "حدث خطأ أثناء الحفظ، حاول مرة أخرى";
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

        private static ProductUnitDto MapInputToUnitDto(ProductUnitInputDto u) => new()
        {
            Id = u.Id ?? 0,
            Name = u.Name,
            Factor = u.Factor,
            SellingPrice = u.SellingPrice,
            IsBase = u.IsBase,
            IsDefaultForSale = u.IsDefaultForSale,
            SortOrder = u.SortOrder,
            Barcodes = u.Barcodes.Select(b => new ProductBarcodeDto
            {
                Id = b.Id ?? 0,
                Barcode = b.Barcode,
                IsPrimary = b.IsPrimary
            }).ToList()
        };

        private static async Task<string?> ReadApiErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(body))
                    return null;
                if (body.Length > 180)
                    body = body[..180] + "…";
                return body.Trim('"');
            }
            catch
            {
                return null;
            }
        }

        private async Task Close()
        {
            if (isSaving) return;
            draftLoaded = false;
            lastEditingId = null;
            await OnClose.InvokeAsync();
        }
    }
}
