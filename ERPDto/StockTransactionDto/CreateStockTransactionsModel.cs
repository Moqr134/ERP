using System.ComponentModel.DataAnnotations;
using Validation.Attribute;

namespace ERPDto.StockTransactionDto
{
    public class CreateStockTransactionsModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار منتج")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار المخزن")]
        public int WarehouseId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        public int Quantity { get; set; }

        [StringValidate(1, 4, false)]
        public string TransactionType { get; set; } = "In";

        [StringValidate(-1, -1, true)]
        public string? Notes { get; set; }

        [StringValidate(-1, 100, true)]
        public string? ReferenceId { get; set; }
    }
}
