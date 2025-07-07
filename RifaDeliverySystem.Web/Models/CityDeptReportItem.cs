namespace RifaDeliverySystem.Web.Models
{
    public class CityDeptReportItem
    {
        public string VendorName { get; set; } = "";
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
