using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Models;

namespace RifaDeliverySystem.Web.Controllers
{
    public class EditionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EditionsController(ApplicationDbContext context) => _context = context;

        // GET: Editions
        public async Task<IActionResult> Index()
        {
            return View(await _context.RaffleEditions
                .OrderBy(e => e.Year)
                .ThenBy(e => e.Number)
                .ToListAsync());
        }

        // GET: Editions/Create
        public IActionResult Create()
        {
            return View(new RaffleEdition { Year = DateTime.Now.Year });
        }

        // POST: Editions/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RaffleEdition edition)
        {
            //if (!ModelState.IsValid) return View(edition);
            _context.RaffleEditions.Add(edition);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Editions/Close/5
        [HttpPost]
        public async Task<IActionResult> Close(int id)
        {
            var ed = await _context.RaffleEditions.FindAsync(id);
            if (ed != null && !ed.IsClosed)
            {
                ed.IsClosed = true;
                ed.ClosedAt = DateTime.Now;
                _context.Update(ed);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
