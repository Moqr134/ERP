using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPDto.ProductsDto
{
    public class ProductStockLadgerDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; }
        public string ReferenceId { get; set; }
        public string Notes { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
