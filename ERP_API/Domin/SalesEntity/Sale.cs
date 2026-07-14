using Common.App;

namespace ERP_API.Domin.SalesEntity
{
    public class Sale : Entity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash";
        public double SubTotal { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public double PaidAmount { get; set; }
        public double ChangeAmount { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Completed";
        public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
    }
}
