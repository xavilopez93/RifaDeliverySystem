using Microsoft.AspNetCore.Mvc;

namespace RifaDeliverySystem.Web.ViewModels.Reports
{
    public class VendorItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int Sold { get; set; }
        public int Returned { get; set; }
        public int Extravio { get; set; }
        public int Robo { get; set; }
        public string Class { get; set; }
    }

}
