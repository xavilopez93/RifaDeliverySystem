// Models/CouponReassignment.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace RifaDeliverySystem.Web.Models
{
    public class CouponReassignment
    {
        public int Id { get; set; }

        [Required]
        public int FromVendorId { get; set; }
        public Vendor FromVendor { get; set; } = null!;

        [Required]
        public int ToVendorId { get; set; }
        public Vendor ToVendor { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int StartNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EndNumber { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string? Reason { get; set; }
    }
}
