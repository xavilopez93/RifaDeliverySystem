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
               // .Include(r => r.AvailableRanges)
                .Include(r => r.Annulments)
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
                .Include(r => r.Annulments)
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
                Date = DateTime.UtcNow,
                AvailableRanges = ranges,//new List<CouponRange> { new CouponRange()},
                Payments = new List<Payment> { new Payment() } // arranca con 1 fila vacía
            };

            return View(model);
        }

        // POST: Renditions/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rendition model)
        {
            // optional: ensure sold <= range capacity



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



            // sync payment methods from checkboxes into JSON
            //model.SyncJsonFromPaymentMethods();
            // calculate commission
            var vendor = await _context.Vendors.FindAsync(model.VendorId);
            var sold = model.CouponsSold;
            var rule = await _context.CommissionRules
                .Where(r =>
                    r.VendorType == vendor.Type &&
                    r.VendorClass == vendor.Class &&
                    r.MinCoupons <= sold &&
                   (r.MaxCoupons == null || r.MaxCoupons >= sold))
                .FirstOrDefaultAsync();

            model.CommissionAmount = rule == null
                ? 0m
                : Math.Round(sold * 10000m * (rule.Percentage / 100m), 0);
            
            model.Balance = sold * 10000m - model.CommissionAmount;
            model.Date = DateTime.UtcNow;
            var rendition = new Rendition();
            rendition = model;    
   
         
            _context.Renditions.Add(rendition);
        
            var ranges = await _context.CouponRanges
          .Where(r => model.RangeIds.Contains(r.Id) && r.RenditionId == null)
          .ToListAsync();
            ranges.ForEach(r => r.Rendition = rendition);

            //model.Payments.ForEach(p => rendition.Payments.Add(p));

            var paymentsCopy = model.Payments.ToList();
            foreach (var p in paymentsCopy)
            {
                p.Rendition = model;
                // si necesitas, también puedes hacer: model.Payments.Add(p);
            }
            await _context.SaveChangesAsync();

            //ViewBag.VendorList = new SelectList(_context.Vendors, "Id", "Name", model.VendorId);
            //ViewBag.CouponRangeList = new SelectList(_context.CouponRanges, "Id", "DisplayName", model.CouponRanges);

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
        .Include(r => r.Annulments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rendition == null)
                return NotFound();


            //ViewBag.Vendors = new SelectList(
            //   await _context.Vendors.OrderBy(v => v.Name).ToListAsync(),
            //   "Id", "Name",
            //   rendition.VendorId);.

            //var vendor = await _context.Vendors.FindAsync(rendition.VendorId);
            //ViewData["VendorName"] = vendor.Name;
    //        var selectedRangeIds = rendition.CouponRanges.Select(r => r.Id).ToList();
    //        var allRanges = await _context.CouponRanges
    //            //.Where(r => r.VendorId == rendition.VendorId && !selectedRangeIds.Contains(r.Id))
    //           // .Where(r => r.VendorId == rendition.VendorId && r.RenditionId == null)
    //           .Where(r => r.Id == selectedRangeIds.Contains(r.Id))
    //.AsNoTracking()
    //            .Select(r => new SelectListItem
    //            {
    //                Value = r.Id.ToString(),
    //                Text = $"{r.StartNumber} - {r.EndNumber}",
    //                Selected = selectedRangeIds.Contains(r.Id)
    //            }).ToListAsync();
           

           ViewData["CouponRanges"] = rendition.CouponRanges.Select(r => new SelectListItem
           {
               Value = r.Id.ToString(),
               Text = $"{r.StartNumber} - {r.EndNumber}",
               Selected = r.Id == rendition.CouponRanges.FirstOrDefault()?.Id // marca el primero como seleccionado 
           }).ToList();

            return View(rendition);
        }

        // POST: Renditions/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rendition model)
        {
            //if (id != model.Id) return NotFound();

            //// optional: sold <= capacity
            ////if (model.CouponRangeId != 0)
            ////{
            ////    var range = await _context.CouponRanges.FindAsync(model.CouponRangeId);
            ////    if (range != null)
            ////    {
            ////        var capacity = range.EndNumber - range.StartNumber + 1;
            ////        if (model.CouponsSold > capacity)
            ////            ModelState.AddModelError(nameof(model.CouponsSold),
            ////                $"No puede vender más de {capacity} cupones en este rango.");
            ////    }
            ////}

            ////if (!ModelState.IsValid)
            ////{
            ////    ViewBag.Vendors = new SelectList(
            ////        await _context.Vendors.OrderBy(v => v.Name).ToListAsync(),
            ////        "Id", "Name",
            ////        model.VendorId);

            ////    ViewBag.Ranges = new SelectList(
            ////        await _context.CouponRanges
            ////            .Include(cr => cr.Vendor)
            ////            .Select(cr => new {
            ////                cr.Id,
            ////                Display = $"{cr.Vendor.Name}: {cr.StartNumber}–{cr.EndNumber}"
            ////            })
            ////            .ToListAsync(),
            ////        "Id",
            ////        "Display",
            ////        model.CouponRangeId);

            ////    return View(model);
            ////}

            //// sync payment methods
            ////model.SyncJsonFromPaymentMethods();

            //// recalc commission & balance
            //var vendor = await _context.Vendors.FindAsync(model.VendorId);
            //var sold = model.CouponsSold;
            //var rule = await _context.CommissionRules
            //    .Where(r =>
            //        r.VendorType == vendor.Type &&
            //        r.VendorClass == vendor.Class &&
            //        r.MinCoupons <= sold &&
            //       (r.MaxCoupons == null || r.MaxCoupons >= sold))
            //    .FirstOrDefaultAsync();

            //model.CommissionAmount = rule == null
            //    ? 0m
            //    : Math.Round(sold * 10000m * (rule.Percentage / 100m), 0);
            //model.Balance = sold * 10000m - model.CommissionAmount;

            //_context.Renditions.Update(model);

            //var oldPayments = _context.Payments.Where(p => p.RenditionId == model.Id);
            //_context.Payments.RemoveRange(oldPayments);

            //// Agregar los nuevos
            //foreach (var p in model.Payments)
            //{
            //    p.RenditionId = model.Id;
            //    _context.Payments.Add(p);
            //}

            //await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));

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

            //for (int i = 0; i < model.Annulments.Count; i++)
            //{
            //    var motivo = (AnnulmentReason)i;
            //    var cantidad = Annu.[i];
            //    // Guardar o asignar a model.Rendition si corresponde
            //}

            // sync payment methods from checkboxes into JSON
            //model.SyncJsonFromPaymentMethods();
            // calculate commission
            var vendor = await _context.Vendors.FindAsync(model.VendorId);
            var sold = model.CouponsSold;
            var rule = await _context.CommissionRules
                .Where(r =>
                    r.VendorType == vendor.Type &&
                    r.VendorClass == vendor.Class &&
                    r.MinCoupons <= sold &&
                   (r.MaxCoupons == null || r.MaxCoupons >= sold))
                .FirstOrDefaultAsync();

            model.CommissionAmount = rule == null
                ? 0m
                : Math.Round(sold * 10000m * (rule.Percentage / 100m), 0);

            model.Balance = sold * 10000m - model.CommissionAmount;
            model.Date = DateTime.UtcNow;
           var rendition = model;
            _context.Renditions.Update(rendition);


            //  var ranges = await _context.CouponRanges
            //.Where(r => model.RangeIds.Contains(r.Id) && r.RenditionId == null)
            //.ToListAsync();
            //  ranges.ForEach(r => r.Rendition = rendition);

            //model.Payments.ForEach(p => rendition.Payments.Add(p));


            var oldPayments = _context.Payments.Where(p => p.RenditionId == model.Id);
            _context.Payments.RemoveRange(oldPayments);

            // Agregar los nuevos
            //foreach (var p in model.Payments)
            //{
            //    p.RenditionId = model.Id;
            //    _context.Payments.Add(p);
            //}
            var paymentsCopy = model.Payments.ToList();
            foreach (var p in paymentsCopy)
            {
                p.Rendition = model;
                // si necesitas, también puedes hacer: model.Payments.Add(p);
            }
            await _context.SaveChangesAsync();

            //ViewBag.VendorList = new SelectList(_context.Vendors, "Id", "Name", model.VendorId);
            //ViewBag.CouponRangeList = new SelectList(_context.CouponRanges, "Id", "DisplayName", model.CouponRanges);

            return RedirectToAction(nameof(Index));
        }

        // GET: Renditions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRanges) 

                //.Include(r => r.AvailableRanges)
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

                // Ahora sí, eliminar la rendición
                _context.Renditions.Remove(model);
                await _context.SaveChangesAsync();
               
                
                //_context.Renditions.Remove(model);
                //await _context.SaveChangesAsync();
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
                .Include(r => r.Annulments)
                //.Include(r=>r.RaffleEdition)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rendition == null) return NotFound();

            // opcional: validar que tenga al menos un método de pago
            //if (!rendition.PaymentMethods.Any())
            //    ModelState.AddModelError("", "Debe seleccionar al menos un método de pago.");

            //if (!ModelState.IsValid)
            //{
            //    // redirige de nuevo a Edit con mensajes
            //    return RedirectToAction(nameof(Edit), new { id });
            //}

            return View(rendition);
        }


        [HttpGet]
        public async Task<IActionResult> PrintReceipt(int id)
        {
            var bytes = await _pdf.GenerateAsync(id);
            return File(bytes, "application/pdf", $"Rendicion_{id}.pdf");
        }

        public IActionResult EmptyPaymentRow()
        {
            return PartialView("_PaymentRow", new Payment());
        }
       

    }
}
