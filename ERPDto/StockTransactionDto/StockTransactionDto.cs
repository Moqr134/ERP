using System;

namespace ERPDto.StockTransactionDto
{
    public class StockTransactionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; }
        public string? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDate { get; set; }
        public int? CreateUserId { get; set; }
    }
}
