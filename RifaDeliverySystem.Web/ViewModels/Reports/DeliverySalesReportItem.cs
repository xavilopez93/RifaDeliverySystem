using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RifaDeliverySystem.Web.ViewModels.Reports
{
    public class DeliverySalesReportItem
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SelectedType { get; set; }
        public string SelectedClass { get; set; } // Added this property to fix the error  
        public IEnumerable<SelectListItem> TypeOptions { get; set; }
        public IEnumerable<SelectListItem> ClassOptions { get; set; }
        public IEnumerable<DeliverySalesReportItem> DeliverySalesReportItems { get; set; }

        public string SellerType { get; set; }
        public string SellerCategory { get; set; }
        public string SellerName { get; set; }
        public int DeliveredCoupons { get; set; }
        public int SoldCoupons { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
    }

}
