using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Models;

namespace RifaDeliverySystem.Web.Controllers
{
    public class VendorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VendorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vendors
        public async Task<IActionResult> Index()
        {
            var list = await _context.Vendors
                                     .OrderBy(v => v.Name)
                                     .ToListAsync();
            return View(list);
        }

        // GET: Vendors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vendor == null) return NotFound();

            return View(vendor);
        }

        // GET: Vendors/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Types = await _context.CommissionRules
                .Select(r => r.VendorType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
            ViewBag.Classes = await _context.CommissionRules
                .Select(r => r.VendorClass)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View();
        }

        // POST: Vendors/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Type,Class,City,Department")] Vendor vendor)
        {
            //if (!ModelState.IsValid)
            //{
                //ViewBag.Types = await _context.CommissionRules
                //    .Select(r => r.VendorType)
                //    .Distinct()
                //    .OrderBy(t => t)
                //    .ToListAsync();
                //ViewBag.Classes = await _context.CommissionRules
                //    .Select(r => r.VendorClass)
                //    .Distinct()
                //    .OrderBy(c => c)
                //    .ToListAsync();

                //return View(vendor);
            //}

            _context.Add(vendor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Vendors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null) return NotFound();

            ViewBag.Types = await _context.CommissionRules
                .Select(r => r.VendorType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
            ViewBag.Classes = await _context.CommissionRules
                .Select(r => r.VendorClass)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(vendor);
        }

        // POST: Vendors/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Class,City,Department")] Vendor vendor)
        {
            if (id != vendor.Id) return NotFound();

            //if (!ModelState.IsValid)
            //{
                //ViewBag.Types = await _context.CommissionRules
                //    .Select(r => r.VendorType)
                //    .Distinct()
                //    .OrderBy(t => t)
                //    .ToListAsync();
                //ViewBag.Classes = await _context.CommissionRules
                //    .Select(r => r.VendorClass)
                //    .Distinct()
                //    .OrderBy(c => c)
                //    .ToListAsync();

                //return View(vendor);
            //}

            try
            {
                _context.Update(vendor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Vendors.AnyAsync(e => e.Id == vendor.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Vendors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vendor == null) return NotFound();

            return View(vendor);
        }

        // POST: Vendors/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor != null)
            {
                _context.Vendors.Remove(vendor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
