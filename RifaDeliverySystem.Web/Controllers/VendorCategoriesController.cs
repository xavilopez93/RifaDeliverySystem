using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Models;

namespace RifaDeliverySystem.Web.Controllers
{
    public class VendorCategoriesController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        public VendorCategoriesController(ApplicationDbContext ctx)
            => _ctx = ctx;

        public async Task<IActionResult> Index()
            => View(await _ctx.VendorCategories.ToListAsync());

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VendorCategory cat)
        {
            //if (!ModelState.IsValid) return View(cat);
            _ctx.VendorCategories.Add(cat);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var cat = await _ctx.VendorCategories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VendorCategory cat)
        {
            if (id != cat.Id) return NotFound();
            //if (!ModelState.IsValid) return View(cat);

            _ctx.VendorCategories.Update(cat);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var cat = await _ctx.VendorCategories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cat = await _ctx.VendorCategories.FindAsync(id);
            if (cat != null)
            {
                _ctx.VendorCategories.Remove(cat);
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
