using System;
using System.ComponentModel.DataAnnotations;

namespace RifaDeliverySystem.Web.Models
{
    public class RaffleEdition
    {
        public int Id { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        public int Year { get; set; }

        public bool IsClosed { get; set; }

        public DateTime? ClosedAt { get; set; }
    }
}
