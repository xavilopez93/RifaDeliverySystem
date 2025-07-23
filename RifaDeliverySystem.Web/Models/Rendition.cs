using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RifaDeliverySystem.Web.Models
{
    public class Rendition
    {
        public int Id { get; set; }


        [Display(Name = "Vendedor")]
        [Required(ErrorMessage = "Debe seleccionar un vendedor")]
        public int VendorId { get; set; }
        [BindNever]
        [ValidateNever]
        public Vendor Vendor { get; set; } = null!;

        //[Required]
        //[Display(Name = "Rango de cupones")]
        //public int CouponRangeId { get; set; }
        //public CouponRange CouponRange { get; set; } = null!;

        public virtual ICollection<CouponRange> CouponRanges { get; set; } = new List<CouponRange>();



        [Required]
        [Display(Name = "Cupones vendidos")]
        public int CouponsSold { get; set; }

        [Required]
        [Display(Name = "Cupones devueltos")]
        public int CouponsReturned { get; set; }

        public int Extravio { get; set; }
        public int Robo { get; set; }


        //[Display(Name = "Anulaciones")]
        //public ICollection<Annulment> Annulments { get; set; } = new List<Annulment>();

        [Display(Name = "Monto de comisión (Gs.)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }

        [Display(Name = "Saldo a favor (Gs.)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public List<Payment> Payments { get; set; } = new();

        //[Required]
        //[Display(Name = "Métodos de pago (JSON)")]
        //public string PaymentMethodsJson { get; set; } = "[]";

        //[NotMapped]
        //[Display(Name = "Métodos de pago")]
        //public List<string> PaymentMethods { get; set; } = new List<string>();

        //[Display(Name = "Otro método de pago")]
        //public string OtherPayment { get; set; } = string.Empty;
        // Multiselección de rangos
        //[Required]                               // valida que al menos uno
        //public List<int> RangeIds { get; set; } = new List<int>();

        //// Poblado por el controlador con rangos disponibles
        //[NotMapped]
        //public IEnumerable<SelectListItem> AvailableRanges { get; set; } =
        //new List<SelectListItem>();
        // Rangos elegidos
        [Required(ErrorMessage = "Seleccione al menos un rango")]
        public List<int> RangeIds { get; set; } = new();

        // Rangos disponibles para el <select multiple>
        public IEnumerable<SelectListItem> AvailableRanges { get; set; } = new List<SelectListItem>();


        [Required]
        [Display(Name = "Fecha")]
        public DateTime Date { get; set; } = DateTime.UtcNow;   // ó DateTimeOffset


        // Llama esto en el GET de Edit para poblar los checkboxes:
        //public void SyncPaymentMethodsFromJson()
        //{
        //    try
        //    {
        //        PaymentMethods = JsonConvert.DeserializeObject<List<string>>(PaymentMethodsJson)
        //                         ?? new List<string>();
        //    }
        //    catch
        //    {
        //        PaymentMethods = new List<string>();
        //    }
        //}

        // Llama esto antes de guardar en Create/Edit POST:
        //public void SyncJsonFromPaymentMethods()
        //{
        //    PaymentMethodsJson = JsonConvert.SerializeObject(PaymentMethods);
        //}
    }
}
