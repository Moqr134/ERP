using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPDto.ProductsDto
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public double CostPrice { get; set; }
        public double SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int CategoriesId { get; set; }
    }
}
