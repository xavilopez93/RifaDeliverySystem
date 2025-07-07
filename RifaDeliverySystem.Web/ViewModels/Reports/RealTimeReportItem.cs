using Microsoft.AspNetCore.Mvc;

namespace RifaDeliverySystem.Web.ViewModels.Reports
{
    public class RealTimeReportItem
    {
        public string BranchName { get; set; }
        public string SellerName { get; set; }
        public int CouponCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

}
