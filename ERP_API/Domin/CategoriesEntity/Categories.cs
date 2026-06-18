using Common.App;
using ERP_API.Domin.ProductEntity;

namespace ERP_API.Domin.CategoriesEntity
{
    public class Categories:Entity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
