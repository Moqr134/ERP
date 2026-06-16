using Common.App;
using ERP_API.Domin.CategoriesEntity;

namespace ERP_API.Domin.ProductEntity
{
    public class Product : Entity
    {
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public double CostPrice { get; set; }
        public double SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public int CategoriesId { get; set; }
        public Categories Categories { get; set; }
    }
}
