using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation.Attribute;

namespace ERPDto.ProductsDto
{
    public class CreateProductModel
    {
        public int Id { get; set; }
        [StringValidate(3,20,false)]
        public string Barcode { get; set; }
        [StringValidate(3, 20, false)]
        public string Name { get; set; }
        [StringValidate(3, 20, false)]
        public string SKU { get; set; }
        public double CostPrice { get; set; }
        public double SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int CategoriesId { get; set; }
        public int? WarehouseId { get; set; }
    }
}
