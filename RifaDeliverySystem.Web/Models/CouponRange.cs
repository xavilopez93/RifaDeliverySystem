using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RifaDeliverySystem.Web.Models
{
    public class CouponRange { 
        public int Id { get; set; } 
        public int VendorId { get; set; }
        [ForeignKey(nameof(VendorId))]
        public Vendor Vendor { get; set; } = null!;

        public int? RenditionId { get; set; }
        public Rendition? Rendition { get; set; }

        public int StartNumber { get; set; } 
        public int EndNumber { get; set; } 
    }
}