using System.ComponentModel.DataAnnotations;

namespace ERPDto.StockTransferDto
{
    public class CreateStockTransferLineDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class CreateStockTransferModel
    {
        [Range(1, int.MaxValue)]
        public int FromWarehouseId { get; set; }

        [Range(1, int.MaxValue)]
        public int ToWarehouseId { get; set; }

        [StringLength(250)]
        public string? Notes { get; set; }

        [MinLength(1, ErrorMessage = "يجب إضافة منتج واحد على الأقل")]
        public List<CreateStockTransferLineDto> Lines { get; set; } = new();
    }

    public class StockTransferLineDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public int Quantity { get; set; }
    }

    public class StockTransferDto
    {
        public int Id { get; set; }
        public string TransferNumber { get; set; } = string.Empty;
        public int FromWarehouseId { get; set; }
        public string FromWarehouseName { get; set; } = string.Empty;
        public int ToWarehouseId { get; set; }
        public string ToWarehouseName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreateDate { get; set; }
        public List<StockTransferLineDto> Lines { get; set; } = new();
    }
}
