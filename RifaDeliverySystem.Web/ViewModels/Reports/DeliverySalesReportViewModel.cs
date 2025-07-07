namespace RifaDeliverySystem.Web.ViewModels.Reports
{
    public class DeliverySalesReportViewModel
    {
        public string SellerType { get; set; }
        public string SellerCategory { get; set; }
        public string SellerName { get; set; }
        public int DeliveredCoupons { get; set; }
        public int SoldCoupons { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string SelectedType { get; set; }
        public string SelectedClass { get; set; }

        // Add the missing properties for TypeOptions and ClassOptions  
        public IEnumerable<SelectListItem> TypeOptions { get; set; }
        public IEnumerable<SelectListItem> ClassOptions { get; set; }
    }

    //public class DeliverySalesReportItem
    //{
    //    public string SellerType { get; set; }
    //    public string SellerCategory { get; set; }
    //    public string SellerName { get; set; }
    //    public int DeliveredCoupons { get; set; }
    //    public int SoldCoupons { get; set; }
    //    public decimal GrossAmount { get; set; }
    //    public decimal CommissionAmount { get; set; }
    //    public decimal NetAmount { get; set; }
    //}
}
