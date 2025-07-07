using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Models;

namespace RifaDeliverySystem.Web.Controllers
{
    public class CommissionRulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CommissionRulesController(ApplicationDbContext context)
            => _context = context;

        // GET: CommissionRules
        public async Task<IActionResult> Index()
            => View(await _context.CommissionRules.ToListAsync());

        // GET: CommissionRules/Create
        public IActionResult Create()
            => View();

        // POST: CommissionRules/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommissionRule rule)
        {
            //if (ModelState.IsValid)
            //{
                _context.Add(rule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            //}
            return View(rule);
        }

        // GET: CommissionRules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var rule = await _context.CommissionRules.FindAsync(id);
            if (rule == null) return NotFound();
            return View(rule);
        }

        // POST: CommissionRules/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CommissionRule rule)
        {
            if (id != rule.Id) return NotFound();
            //if (!ModelState.IsValid) return View(rule);

            try
            {
                _context.Update(rule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.CommissionRules.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: CommissionRules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var rule = await _context.CommissionRules
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rule == null) return NotFound();
            return View(rule);
        }

        // POST: CommissionRules/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rule = await _context.CommissionRules.FindAsync(id);
            if (rule != null)
            {
                _context.CommissionRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
