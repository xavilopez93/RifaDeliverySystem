using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace RifaDeliverySystem.Web.ViewModels.Reports
{
    public class VendorSummaryGroup
    {
        public string GroupName { get; set; } = "";
        public int TotalSold { get; set; }
        public decimal TotalCommission { get; set; }
    }

    public class VendorSummaryFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? SelectedTypeId { get; set; }
        public int? SelectedClassId { get; set; }

        public IEnumerable<SelectListItem> TypeOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ClassOptions { get; set; } = new List<SelectListItem>();

        public List<VendorSummaryGroup> Results { get; set; } = new();
    }
}
