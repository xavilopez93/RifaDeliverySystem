using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RifaDeliverySystem.Web.Models;
using System.Diagnostics;

namespace RifaDeliverySystem.Web.ViewModels.Reports
{
    public class VendorSummaryFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Vendor? SelectedType { get; set; }
        public VendorCategory? SelectedClass { get; set; }

        public IEnumerable<SelectListItem> TypeOptions { get; set; }
        public IEnumerable<SelectListItem> ClassOptions { get; set; }

        public IEnumerable<DelimitedListTraceListener> Results { get; set; }
    }

}
