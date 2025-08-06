using Microsoft.AspNetCore.Mvc;

namespace RifaDeliverySystem.Web.ViewModels.Reports
{
    public class VendorByClassGroupViewModel
    {
        public string Class { get; set; } = string.Empty;

        public List<VendorItemViewModel> Vendors { get; set; } = new();

        public int TotalSold => Vendors.Sum(v => v.Sold);
        public int TotalReturned => Vendors.Sum(v => v.Returned);
        public int TotalExtravio => Vendors.Sum(v => v.Extravio);
        public int TotalRobo => Vendors.Sum(v => v.Robo);
    }

}
