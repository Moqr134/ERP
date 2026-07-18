using Common.App;
using ERP_API.Domin.ProductEntity;

namespace ERP_API.Domin.WarehouseEntity
{
    public class Warehouse : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<WarehouseStock> Stocks { get; set; } = new List<WarehouseStock>();
    }
}
