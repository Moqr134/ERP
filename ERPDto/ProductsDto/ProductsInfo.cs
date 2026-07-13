using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPDto.ProductsDto
{
    public class ProductsInfo
    {
        public int TotalProducts { get; set; } = 0;
        public int PageCount { get; set; } = 0;
        public int ProductsCountLissMinStock { get; set; } = 0;
        public int ProductsStockOut { get; set; } = 0;
        public double ProductsCostCount { get; set; } = 0;
    }
}
