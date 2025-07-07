namespace RifaDeliverySystem.Web.Models
{
    public class TypeClassVendorReportItem
    {
        public string VendorName { get; set; } = null!;
        public int Delivered { get; set; }
        public int Sold { get; set; }
        public int Returned { get; set; }
        public int Annulled { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public bool Closed { get; set; }
        public decimal SalePercentage { get; set; }
    }
}
