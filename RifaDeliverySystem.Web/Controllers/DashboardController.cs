using Microsoft.AspNetCore.Mvc;
using RifaDeliverySystem.Web.Data;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace RifaDeliverySystem.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DashboardHub> _hub;
        public DashboardController(ApplicationDbContext context, IHubContext<DashboardHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task<IActionResult> Index()
        {
            // initial data load
            var totalVendors = await _context.Vendors.CountAsync();
            var totalSold = await _context.Renditions.SumAsync(r => r.CouponsSold);
            var totalBalance = await _context.Renditions.SumAsync(r => r.Balance);
            ViewBag.TotalVendors = totalVendors;
            ViewBag.TotalSold = totalSold;
            ViewBag.TotalBalance = totalBalance;
            return View();
        }

        // call this method after each Create/Edit/Delete in RenditionsController
        public async Task BroadcastUpdate()
        {
            var totalSold = await _context.Renditions.SumAsync(r => r.CouponsSold);
            var totalBalance = await _context.Renditions.SumAsync(r => r.Balance);
            await _hub.Clients.All.SendAsync("ReceiveUpdate", totalSold, totalBalance);
        }
    }
}
