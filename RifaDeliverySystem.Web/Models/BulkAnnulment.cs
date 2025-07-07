using System;
using System.ComponentModel.DataAnnotations;

namespace RifaDeliverySystem.Web.Models
{
    public class BulkAnnulment
    {
        public int Id { get; set; }

        [Required]
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int StartNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EndNumber { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required, StringLength(250)]
        public string Reason { get; set; } = null!;
    }
}
