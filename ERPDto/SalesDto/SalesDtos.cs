using System.ComponentModel.DataAnnotations;

namespace ERPDto.SalesDto
{
    public class CompleteSaleLineDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        /// <summary>Optional. When set, uses that packaging unit; otherwise base/default unit.</summary>
        public int? ProductUnitId { get; set; }

        /// <summary>Quantity in the selected unit (cartons, pieces…).</summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>Optional override; server uses unit SellingPrice if null/0.</summary>
        [Range(0, double.MaxValue)]
        public double? UnitPrice { get; set; }

        public string? Barcode { get; set; }
    }

    public class CompleteSaleModel
    {
        [MinLength(1, ErrorMessage = "يجب إضافة منتج واحد على الأقل")]
        public List<CompleteSaleLineDto> Lines { get; set; } = new();

        [Range(0, double.MaxValue)]
        public double Discount { get; set; }

        [Range(0, double.MaxValue)]
        public double PaidAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "Cash";

        [StringLength(250)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار مخزن البيع")]
        public int WarehouseId { get; set; }
    }

    public class SaleLineDto
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
        public double UnitPrice { get; set; }
        public double LineTotal { get; set; }
    }

    public class SaleDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public double SubTotal { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public double PaidAmount { get; set; }
        public double ChangeAmount { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public DateTime CreateDate { get; set; }
        public int? CreateUserId { get; set; }
        public List<SaleLineDto> Lines { get; set; } = new();
    }

    public class SalesListResponse
    {
        public List<SaleDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }
}
