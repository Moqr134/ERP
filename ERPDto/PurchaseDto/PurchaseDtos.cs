using System.ComponentModel.DataAnnotations;

namespace ERPDto.PurchaseDto
{
    public class CompletePurchaseLineDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        /// <summary>Optional packaging unit; otherwise base unit.</summary>
        public int? ProductUnitId { get; set; }

        /// <summary>Quantity in the selected unit (cartons, pieces…).</summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>Cost per selected unit. Defaults to product CostPrice × factor when null.</summary>
        [Range(0, double.MaxValue)]
        public double? UnitCost { get; set; }

        public string? Barcode { get; set; }
    }

    public class CompletePurchaseReceiptModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار المورد")]
        public int SupplierId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار مخزن الاستلام")]
        public int WarehouseId { get; set; }

        [MinLength(1, ErrorMessage = "يجب إضافة منتج واحد على الأقل")]
        public List<CompletePurchaseLineDto> Lines { get; set; } = new();

        [Range(0, double.MaxValue)]
        public double Discount { get; set; }

        [StringLength(250)]
        public string? Notes { get; set; }

        [StringLength(40)]
        public string? SupplierInvoiceNumber { get; set; }
    }

    public class PurchaseReceiptLineDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public int Quantity { get; set; }
        public int BaseQuantity { get; set; }
        public string UnitName { get; set; } = "مفرد";
        public int UnitFactor { get; set; } = 1;
        public int? ProductUnitId { get; set; }
        public double UnitCost { get; set; }
        public double LineTotal { get; set; }
    }

    public class PurchaseReceiptDto
    {
        public int Id { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierInvoiceNumber { get; set; }
        public double SubTotal { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public DateTime CreateDate { get; set; }
        public int? CreateUserId { get; set; }
        public List<PurchaseReceiptLineDto> Lines { get; set; } = new();
    }

    public class PurchaseReceiptListResponse
    {
        public List<PurchaseReceiptDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }
}
