using ClosedXML;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Models;
using System.Reflection.Emit;

namespace RifaDeliverySystem.Web.Data
{
    public class ApplicationUser : IdentityUser
    {
        // Puedes agregar campos extra, p.ej. FullName, Department, etc.
       //public string? FullName { get; set; } 
    }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public DbSet<CouponRange> CouponRanges { get; set; }
        public DbSet<Rendition> Renditions { get; set; }
        public DbSet<Annulment> Annulments { get; set; }
        public DbSet<CommissionRule> CommissionRules { get; set; }
        public DbSet<VendorCategory> VendorCategories { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        // Data/ApplicationDbContext.cs
        public DbSet<CouponReassignment> CouponReassignments { get; set; }

        public DbSet<BulkAnnulment> BulkAnnulments { get; set; }
        public DbSet<RaffleEdition> RaffleEditions { get; set; }

        public DbSet<Payment> Payments { get; set; }  




        protected override void OnModelCreating(ModelBuilder mb)
        {

            base.OnModelCreating(mb);
            //base.OnModelCreating(mb);
            //mb.Entity<Rendition>()
            //  .HasMany(r=>r.Annulments)
            //  .WithOne(a=>a.Rendition)
            //  .HasForeignKey(a=>a.RenditionId)
            //  .OnDelete(DeleteBehavior.Cascade);
            //    mb.Entity<Rendition>().Ignore(r => r.PaymentMethods);
            //    //.Ignore(r=>r.OtherPayment);
            // Rendition / Annulment cascade
            //        mb.Entity<Rendition>()
            //          .HasMany(r => r.Annulments)
            //          .WithOne(a => a.Rendition)
            //          .HasForeignKey(a => a.RenditionId)
            //          .OnDelete(DeleteBehavior.Cascade);
            //      //  mb.Entity<Rendition>()
            //      //.HasOne(r => r.CouponRange)
            //      //.WithMany()               // or .WithMany(cr => cr.Renditions) if you added that nav
            //      //.HasForeignKey(r => r.CouponRangeId)
            //      //.OnDelete(DeleteBehavior.Restrict);

            //        mb.Entity<Rendition>()
            //.HasMany(r => r.AvailableRanges)
            //.WithOne(cr => cr.GetMethod)
            //.HasForeignKey(cr => cr.RenditionId)
            //.OnDelete(DeleteBehavior.Restrict);

            // ►  1 ── N CouponRange
            mb.Entity<CouponRange>()
                .HasOne(cr => cr.Vendor)
                .WithMany(v => v.CouponRanges)
                .HasForeignKey(cr => cr.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ► Rendition 1 ── N CouponRange  (FK vive en CouponRange)
            //mb.Entity<CouponRange>()
            //    .HasOne(cr => cr.Rendition)
            //    .WithMany(r => r.AvailableRanges)
            //    .HasForeignKey(cr => cr.RenditionId)   // ← Nullable<int>
            //    .OnDelete(DeleteBehavior.Restrict);    // evita cascada accidental

            // (opcional) índice compuesto para evitar solapes
            mb.Entity<CouponRange>()
                .HasIndex(cr => new { cr.VendorId, cr.StartNumber, cr.EndNumber });


            mb.Entity<CouponReassignment>()
            .HasOne(r => r.FromVendor)
            .WithMany()
            .HasForeignKey(r => r.FromVendorId)
            // keep cascade here if you like:
            .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CouponReassignment>()
                .HasOne(r => r.ToVendor)
                .WithMany()
                .HasForeignKey(r => r.ToVendorId)
                // switch this FK to Restrict (no cascade)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<BulkAnnulment>()
                  .HasOne(b => b.Vendor)
                  .WithMany()                   // no tienes colección inversa
                  .HasForeignKey(b => b.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Payment>(b =>
            {
                b.HasOne(p => p.Rendition)
                 .WithMany(r => r.Payments)
                 .HasForeignKey(p => p.RenditionId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            });
            mb.Ignore<SelectListItem>();
            mb.Ignore<SelectListGroup>();

        }
    }

}
