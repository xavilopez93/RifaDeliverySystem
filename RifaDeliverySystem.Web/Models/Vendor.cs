using System.ComponentModel.DataAnnotations;

namespace RifaDeliverySystem.Web.Models
{
    public class Vendor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "El tipo de vendedor es obligatorio")]
        public string Type { get; set; } = null!;

        [Required(ErrorMessage = "La clase de vendedor es obligatoria")]
        public string Class { get; set; } = null!;

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "El departamento es obligatorio")]
        public string Department { get; set; } = null!;
        public ICollection<CouponRange> CouponRanges { get; set; }
     = new List<CouponRange>();
    }
}
