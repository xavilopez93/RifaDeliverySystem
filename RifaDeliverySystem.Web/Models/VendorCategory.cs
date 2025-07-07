using System.ComponentModel.DataAnnotations.Schema;

namespace RifaDeliverySystem.Web.Models
{
    public class VendorCategory
    {
        public int Id { get; set; }

        // e.g. “Escuelas Comunitarias de Música”
        public string Type { get; set; } = null!;

        // e.g. “ECM”
        public string Class { get; set; } = null!;

        // Base commission % (for simple cases; you can extend this later)
        public decimal CommissionRate { get; set; }

        // For display in dropdown
        [NotMapped]
        public string DisplayName => $"{Type} – {Class} ({CommissionRate:F0}% base)";
    }
}
