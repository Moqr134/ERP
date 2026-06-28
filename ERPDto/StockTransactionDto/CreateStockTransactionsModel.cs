using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation.Attribute;

namespace ERPDto.StockTransactionDto
{
    public class CreateStockTransactionsModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        [StringValidate(1, 4, true)]
        public string TransactionType { get; set; }
        [StringValidate(-1, -1, true)]
        public string? Notes { get; set; }
        public string? ReferenceId { get; set; }
    }
}
