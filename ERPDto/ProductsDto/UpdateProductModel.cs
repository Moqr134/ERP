using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPDto.ProductsDto
{
    public class UpdateProductModel
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string SKU { get; set; }
        public double Price { get; set; }
        public string Barcode { get; set; }
        public int CategoryId { get; set; }
    }
}
