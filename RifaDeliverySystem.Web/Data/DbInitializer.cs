using System.Linq;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Models;

namespace RifaDeliverySystem.Web.Data;
public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext ctx)
    {
        ctx.Database.Migrate();

        // Only seed once
        if (!ctx.CommissionRules.Any())
        {
            ctx.CommissionRules.AddRange(new[]
            {
              // Escuelas Comunitarias de Música (ECM)
                    new CommissionRule { VendorType="Escuelas Comunitarias de Música", VendorClass="ECM", MinCoupons=1,    MaxCoupons=590,  Percentage=0.40m },
                    new CommissionRule { VendorType="Escuelas Comunitarias de Música", VendorClass="ECM", MinCoupons=591,  MaxCoupons=1090, Percentage=0.45m },
                    new CommissionRule { VendorType="Escuelas Comunitarias de Música", VendorClass="ECM", MinCoupons=1091, MaxCoupons=null, Percentage=0.50m },

                    // Voluntarios Corporativos – Banco Familiar, Alex S.A., Farmaoliva
                    new CommissionRule { VendorType="Voluntarios Corporativos", VendorClass="Banco Familiar", MinCoupons=1,    MaxCoupons=1000, Percentage=0.20m },
                    new CommissionRule { VendorType="Voluntarios Corporativos", VendorClass="Alex S.A.",         MinCoupons=1,    MaxCoupons=1000, Percentage=0.20m },
                    new CommissionRule { VendorType="Voluntarios Corporativos", VendorClass="Farmaoliva",        MinCoupons=1,    MaxCoupons=1000, Percentage=0.20m },

                    // Voluntarios Corporativos – Biggie and similar tier-two entities
                    new CommissionRule { VendorType="Voluntarios Corporativos", VendorClass="Biggie",           MinCoupons=1,    MaxCoupons=1000, Percentage=0.20m },
                    new CommissionRule { VendorType="Voluntarios Corporativos", VendorClass="Biggie",           MinCoupons=1001, MaxCoupons=null, Percentage=0.30m },
                    // Repeat for Salemma, Gran Vía, Rapidoc, Negofin, Peluquería Galana, Hair Lab, Tesacom, Lasca, Bea Accesorios
                    // (you can loop these names rather than write them all out)

                    // Voluntarios Personales Varios
                    new CommissionRule { VendorType="Voluntarios Personales Varios", VendorClass="Varios", MinCoupons=1,    MaxCoupons=1000, Percentage=0.20m },
                    new CommissionRule { VendorType="Voluntarios Personales Varios", VendorClass="Varios", MinCoupons=1001, MaxCoupons=null, Percentage=0.30m },

                    // Asociaciones Afines – Colegios
                    new CommissionRule { VendorType="Asociaciones Afines", VendorClass="Colegios", MinCoupons=1,    MaxCoupons=1000, Percentage=0.30m },
                    new CommissionRule { VendorType="Asociaciones Afines", VendorClass="Colegios", MinCoupons=1001, MaxCoupons=null, Percentage=0.35m },
                    // Asociaciones Afines – Grupo Scout
                    new CommissionRule { VendorType="Asociaciones Afines", VendorClass="Grupo Scout", MinCoupons=1,    MaxCoupons=1000, Percentage=0.40m },
                    new CommissionRule { VendorType="Asociaciones Afines", VendorClass="Grupo Scout", MinCoupons=1001, MaxCoupons=null, Percentage=0.45m },
                    // Asociaciones Afines – El Mejor
                    new CommissionRule { VendorType="Asociaciones Afines", VendorClass="El Mejor", MinCoupons=1,    MaxCoupons=1000, Percentage=0.40m },
                    new CommissionRule { VendorType="Asociaciones Afines", VendorClass="El Mejor", MinCoupons=1001, MaxCoupons=null, Percentage=0.45m },

                    // Instructores & Staff
                    new CommissionRule { VendorType="Instructores", VendorClass="Instructores", MinCoupons=1,    MaxCoupons=1000, Percentage=0.40m },
                    new CommissionRule { VendorType="Instructores", VendorClass="Instructores", MinCoupons=1001, MaxCoupons=null, Percentage=0.50m },
                    new CommissionRule { VendorType="Staff",       VendorClass="Staff",       MinCoupons=1,    MaxCoupons=1000, Percentage=0.40m },
                    new CommissionRule { VendorType="Staff",       VendorClass="Staff",       MinCoupons=1001, MaxCoupons=null, Percentage=0.50m },

                    // Asociados Sonidos de la Tierra (with commission)
                    new CommissionRule { VendorType="Asociados Sonidos de la Tierra", VendorClass="Asociados",    MinCoupons=1,    MaxCoupons=1000, Percentage=0.40m },
                    new CommissionRule { VendorType="Asociados Sonidos de la Tierra", VendorClass="Asociados",    MinCoupons=1001, MaxCoupons=null, Percentage=0.50m },
                    // Asociados Sonidos de la Tierra – no commission
                    new CommissionRule { VendorType="Asociados Sonidos de la Tierra", VendorClass="Asociados 22", MinCoupons=0,    MaxCoupons=null, Percentage=0.00m },

                    // Padrinos Culturales – no commission
                    new CommissionRule { VendorType="Padrinos Culturales", VendorClass="Padrinos Culturales", MinCoupons=0, MaxCoupons=null, Percentage=0.00m },

                    // Digitales – card fees (4.5%)
                    new CommissionRule { VendorType="Digitales", VendorClass="Web: Bancard",       MinCoupons=0, MaxCoupons=null, Percentage=0.045m },
                    new CommissionRule { VendorType="Digitales", VendorClass="Bancard App Pago Móvil", MinCoupons=0, MaxCoupons=null, Percentage=0.045m },
                    new CommissionRule { VendorType="Digitales", VendorClass="Bancard: Infonet Cobranzas", MinCoupons=0, MaxCoupons=null, Percentage=0.045m },
                    // Digitales – WhatsApp institucional (no commission)
                    new CommissionRule { VendorType="Digitales", VendorClass="WhatsApp institucional", MinCoupons=0, MaxCoupons=null, Percentage=0.00m },

            });
        }

        ctx.SaveChanges();
    }
}


