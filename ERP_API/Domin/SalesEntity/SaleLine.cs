using Common.App;
using ERP_API.Domin.ProductEntity;

namespace ERP_API.Domin.SalesEntity
{
    public class SaleLine : Entity
    {
        public int SaleId { get; set; }
        public Sale Sale { get; set; } = null!;
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Barcode { get; set; }

        /// <summary>Sold package qty (e.g. 2 cartons).</summary>
        public int Quantity { get; set; }

        /// <summary>Base units deducted from stock (Quantity × UnitFactor).</summary>
        public int BaseQuantity { get; set; }

        public string UnitName { get; set; } = "مفرد";
        public int UnitFactor { get; set; } = 1;
        public int? ProductUnitId { get; set; }

        public double UnitPrice { get; set; }
        public double LineTotal { get; set; }
    }
}
