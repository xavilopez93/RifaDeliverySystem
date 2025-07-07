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
    public class CouponRangesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CouponRangesController(ApplicationDbContext context) => _context = context;
     
        public async Task<IActionResult> Index() => View(await _context.CouponRanges.Include(c => c.Vendor).ToListAsync());

        public async Task<IActionResult> Create()
        {
            ViewBag.Vendors = new SelectList(
               await _context.Vendors
                   .OrderBy(v => v.Name)
                   .ToListAsync(),
               "Id",
               "Name");

          

            return View(new CouponRange());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CouponRange range)
        {
            //if (ModelState.IsValid)
            //{
            _context.Add(range);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            //}
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            return View(range);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var range = await _context.CouponRanges.FindAsync(id);
            if (range == null) return NotFound();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            return View(range);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CouponRange range)
        {
            if (id != range.Id) return NotFound();
            //if (ModelState.IsValid)
            //{
                _context.Update(range);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            //}
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            return View(range);
        }

        public async Task<IActionResult> Delete(int? id)
        {
           
            if (id == null) return NotFound();
            var range = await _context.CouponRanges.FirstOrDefaultAsync(m => m.Id == id);
            if (range == null) return NotFound();
            return View(range);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var range = await _context.CouponRanges.FindAsync(id);
            if (range != null)
            {
                _context.CouponRanges.Remove(range);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
