using DocumentFormat.OpenXml.InkML;
using iText.Kernel.Pdf.Action;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Models;
using RifaDeliverySystem.Web.Services.Pdf;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RifaDeliverySystem.Web.Controllers
{
    public class RenditionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReceiptPdfService _pdf;
        public RenditionsController(ApplicationDbContext context, IReceiptPdfService pdf)
        {
            _context = context;
            _pdf = pdf;
        }

        // GET: Renditions
        public async Task<IActionResult> Index()
        {
            var renditions = await _context.Renditions
                .Include(r => r.Vendor)
                 .Include(r => r.Payments)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            return View(renditions);
        }

        // GET: Renditions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var rendition = await _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.AvailableRanges)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rendition == null) return NotFound();
            return View(rendition);
        }

        // GET: Renditions/Create
        public async Task<IActionResult> Create(int vendorId)
        {
            ViewBag.Vendors = new SelectList(
                await _context.Vendors
                    .OrderBy(v => v.Name)
                    .ToListAsync(),
                "Id",
                "Name", vendorId);

            var ranges = await _context.CouponRanges
                   .Where(r => r.VendorId == vendorId && r.RenditionId == null)
                   .Select(r => new SelectListItem
                   {
                       Value = r.Id.ToString(),
                       Text = $"{r.StartNumber}-{r.EndNumber}"
                   })
                   .ToListAsync();

            var model = new Rendition
            {
                VendorId = vendorId,
                // Inicializar la fecha con la hora local para permitir que el usuario la ajuste.
                Date = DateTime.Now,
                AvailableRanges = ranges,
                // Se inicia con una fila de pago vacía.
                Payments = new List<Payment>() // { new Payment() }
            };

            return View(model);
        }

        // POST: Renditions/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rendition model)
        {

            if (!ModelState.IsValid)
            {
                model.AvailableRanges = await _context.CouponRanges
            .Where(r => r.VendorId == model.VendorId && r.RenditionId == null)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.StartNumber}-{r.EndNumber}"
            }).ToListAsync();

                ViewBag.Vendors = new SelectList(
                    await _context.Vendors.OrderBy(v => v.Name).ToListAsync(),
                    "Id", "Name", model.VendorId);
                return View(model);
            }

            // cálculo de comisión y saldo basado en cupones netos (vendidos - devueltos - extraviados - robados)
            var vendor = await _context.Vendors.FindAsync(model.VendorId);
            var totalCoupons = model.CouponsSold;
            var returned = model.CouponsReturned;
            var extravio = model.Extravio;
            var robo = model.Robo;
            var netCoupons = totalCoupons - returned - extravio - robo;
            if (netCoupons < 0) netCoupons = 0;

            var rule = await _context.CommissionRules
                .Where(r =>
                    r.VendorType == vendor.Type &&
                    r.VendorClass == vendor.Class &&
                    r.MinCoupons <= totalCoupons &&
                   (r.MaxCoupons == null || r.MaxCoupons >= totalCoupons))
                .FirstOrDefaultAsync();

            var revenue = netCoupons * 10000m;
            model.CommissionAmount = rule == null
                ? 0m
                : Math.Round(revenue * (rule.Percentage / 100m), 0);
            
            model.Balance = revenue - model.CommissionAmount;
            // Construir una nueva instancia de Rendition para evitar adjuntar propiedades no deseadas (como AvailableRanges).
            var rendition = new Rendition
            {
                VendorId = model.VendorId,
                Date = model.Date,
                CouponsSold = model.CouponsSold,
                CouponsReturned = model.CouponsReturned,
                Extravio = model.Extravio,
                Robo = model.Robo,
                CommissionAmount = model.CommissionAmount,
                Balance = model.Balance,
                Payments = new List<Payment>(),
            };

            _context.Renditions.Add(rendition);

            // Asignar rangos seleccionados a la nueva rendición
            var ranges = await _context.CouponRanges
                .Where(r => model.RangeIds.Contains(r.Id) && r.RenditionId == null)
                .ToListAsync();
            foreach (var r in ranges)
            {
                r.Rendition = rendition;
            }

            if (model.Payments != null)
            {
                foreach (var p in model.Payments)
                {
                    if (p != null && p.Amount > 0)
                    {
                        rendition.Payments.Add(new Payment
                        {
                            Type = p.Type,
                            Amount = p.Amount,
                            ReceiptNumber = p.ReceiptNumber
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Renditions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Renditions == null)
                return NotFound();

            var rendition = await _context.Renditions
                .Include(r => r.CouponRanges)
                .Include(r => r.Vendor)
                .Include(r => r.Payments)   
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rendition == null)
                return NotFound();

            // Marcar todos los rangos asignados como seleccionados para que se muestren correctamente en la vista.
            ViewData["CouponRanges"] = rendition.CouponRanges.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.StartNumber} - {r.EndNumber}",
                Selected = true
            }).ToList();

            return View(rendition);
        }

        // POST: Renditions/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rendition model)
        {
            
            if (!ModelState.IsValid)
            {
                model.AvailableRanges = await _context.CouponRanges
            .Where(r => r.VendorId == model.VendorId && r.RenditionId == null)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.StartNumber}-{r.EndNumber}"
            }).ToListAsync();

                ViewBag.Vendors = new SelectList(
                    await _context.Vendors.OrderBy(v => v.Name).ToListAsync(),
                    "Id", "Name", model.VendorId);
                return View(model);
            }

            // Cargamos la rendición existente con sus rangos y pagos para actualizarla correctamente
            var dbRendition = await _context.Renditions
                .Include(r => r.CouponRanges)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == model.Id);
            if (dbRendition == null)
                return NotFound();

            // Calcular comisión y saldo basado en cupones netos (vendidos - devueltos - extraviados - robados)
            var vendor = await _context.Vendors.FindAsync(model.VendorId);
            var totalCoupons = model.CouponsSold;
            var returned = model.CouponsReturned;
            var extravio = model.Extravio;
            var robo = model.Robo;
            var netCoupons = totalCoupons - returned - extravio - robo;
            if (netCoupons < 0) netCoupons = 0;

            var rule = await _context.CommissionRules
                .Where(r =>
                    r.VendorType == vendor.Type &&
                    r.VendorClass == vendor.Class &&
                    r.MinCoupons <= totalCoupons &&
                   (r.MaxCoupons == null || r.MaxCoupons >= totalCoupons))
                .FirstOrDefaultAsync();

            var revenue = netCoupons * 10000m;
            var commission = rule == null
                ? 0m
                : Math.Round(revenue * (rule.Percentage / 100m), 0);

            // Actualizar campos simples de la rendición existente
            dbRendition.CouponsSold = model.CouponsSold;
            dbRendition.CouponsReturned = model.CouponsReturned;
            dbRendition.Extravio = model.Extravio;
            dbRendition.Robo = model.Robo;
            dbRendition.Date = model.Date.ToUniversalTime();
            dbRendition.CommissionAmount = commission;
            dbRendition.Balance = revenue - commission;

            // Actualizar los rangos: eliminar los que ya no están seleccionados
            foreach (var range in dbRendition.CouponRanges.ToList())
            {
                if (!model.RangeIds.Contains(range.Id))
                {
                    range.RenditionId = null;
                    dbRendition.CouponRanges.Remove(range);
                }
            }
            // Asignar los rangos que se seleccionaron y aún no estaban asignados
            var rangesToAdd = await _context.CouponRanges
                .Where(r => model.RangeIds.Contains(r.Id) && r.RenditionId == null)
                .ToListAsync();
            foreach (var r in rangesToAdd)
            {
                r.Rendition = dbRendition;
            }

            // Actualizar los pagos: eliminar los pagos antiguos y añadir los nuevos
            _context.Payments.RemoveRange(dbRendition.Payments);
            //dbRendition.Payments.Clear();

            if (model.Payments != null)
            {
                foreach (var p in model.Payments)
                {
                    if (p != null && p.Amount > 0)
                    {
                        dbRendition.Payments.Add(new Payment
                        {
                            Type = p.Type,
                            Amount = p.Amount,
                            ReceiptNumber = p.ReceiptNumber
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Renditions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRanges) 
                .FirstOrDefaultAsync(r => r.Id == id);

            if (model == null) return NotFound();
            return View(model);
        }

        // POST: Renditions/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var model = await _context.Renditions.FirstOrDefaultAsync(m => m.Id == id);
            if (model != null)
            {
                // Desvincular los coupon ranges asociados
                var couponRanges = await _context.CouponRanges
                    .Where(c => c.RenditionId == model.Id)
                    .ToListAsync();

                foreach (var range in couponRanges)
                {
                    range.RenditionId = null;
                }
                _context.CouponRanges.UpdateRange(couponRanges);
                _context.Renditions.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Renditions/Comprobante/5
        public async Task<IActionResult> Comprobante(int? id)
        {
            if (id == null) return NotFound();

            var rendition = await _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.AvailableRanges)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rendition == null) return NotFound();
            return View(rendition);
        }


        [HttpGet]
        public async Task<IActionResult> PrintReceipt(int id)
        {
            var bytes = await _pdf.GenerateAsync(id);
            return File(bytes, "application/pdf", $"Rendicion_{id}.pdf");
        }



        [HttpGet]
        public IActionResult EmptyPaymentRow()
        {
            return PartialView("_PaymentRow", new Payment());
        }
       

    }
}
