// Models/Payment.cs
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RifaDeliverySystem.Web.Models
{
    public enum PaymentType
    {
        [Display(Name = "Efectivo")] Cash = 1,
        [Display(Name = "Transferencia")] Transfer,
        [Display(Name = "Depósito bancario")] Deposit,
        [Display(Name = "Giro Tigo / Billetera")] Mobile,
        [Display(Name = "Otro")] Other
    }

    public class Payment
    {
        public int Id { get; set; }

        /*-- FK hacia la rendición -----------------*/
    
        public int RenditionId { get; set; }
        [BindNever]
        [ValidateNever]
        public Rendition Rendition { get; set; } = null!;

        /*-- Datos propios -------------------------*/
        [Required]
        public PaymentType Type { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto (Gs.)")]
        public decimal Amount { get; set; }

        [Display(Name = "N.º comprobante / referencia")]
        [StringLength(50)]
        public string? ReceiptNumber { get; set; }
    }
}
