using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPDto.ProductsDto
{
    public class ProductsInfo
    {
        public int TotalProducts { get; set; }
        public int ProductsCountLissMinStock { get; set; }
        public int ProductsStockOut { get; set; }
        public double ProductsCostCount {  get; set; }
    }
}
