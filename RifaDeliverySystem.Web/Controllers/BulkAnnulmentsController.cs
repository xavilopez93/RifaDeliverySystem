using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Models;

namespace RifaDeliverySystem.Web.Controllers
{
    public class BulkAnnulmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BulkAnnulmentsController(ApplicationDbContext context) => _context = context;

        // GET: BulkAnnulments
        public async Task<IActionResult> Index()
        {
            var items = await _context.BulkAnnulments
                .Include(b => b.Vendor)
                .OrderByDescending(b => b.Date)
                .ToListAsync();
            return View(items);
        }

        // GET: BulkAnnulments/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Vendors = new SelectList(await _context.Vendors.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: BulkAnnulments/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BulkAnnulment model)
        {
            if (model.EndNumber < model.StartNumber)
            {
                ModelState.AddModelError("", "El número final debe ser mayor o igual al inicial.");
            }
            //if (!ModelState.IsValid)
            //{
            //    ViewBag.Vendors = new SelectList(await _context.Vendors.ToListAsync(), "Id", "Name", model.VendorId);
            //    return View(model);
            //}

            _context.BulkAnnulments.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
