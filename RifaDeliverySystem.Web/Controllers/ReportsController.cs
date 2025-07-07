using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Models;

namespace RifaDeliverySystem.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportsController(ApplicationDbContext context)
            => _context = context;

        // --- By Type/Class/Vendor ---
        [HttpGet]
        public async Task<IActionResult> ByTypeClassVendor()
        {
            ViewBag.Types = await _context.CommissionRules.Select(r => r.VendorType).Distinct().ToListAsync();
            ViewBag.Classes = await _context.CommissionRules.Select(r => r.VendorClass).Distinct().ToListAsync();
            return View(new List<TypeClassVendorReportItem>());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ByTypeClassVendor(string type, string vendorClass)
        {
            ViewBag.Types = await _context.CommissionRules.Select(r => r.VendorType).Distinct().ToListAsync();
            ViewBag.Classes = await _context.CommissionRules.Select(r => r.VendorClass).Distinct().ToListAsync();

            var query = _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRanges)
                .Include(r => r.Annulments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(r => r.Vendor.Type == type);
            if (!string.IsNullOrEmpty(vendorClass))
                query = query.Where(r => r.Vendor.Class == vendorClass);

            var data = await query
                .GroupBy(r => new { r.Vendor.Id, r.Vendor.Name })
                .Select(g => new TypeClassVendorReportItem
                {
                    VendorName = g.Key.Name,
                    //Delivered = g.Sum(r => r.CouponRanges.EndNumber - r.CouponRanges.StartNumber + 1),
                    Sold = g.Sum(r => r.CouponsSold),
                    Returned = g.Sum(r => r.CouponsReturned),
                    Annulled = g.SelectMany(r => r.Annulments).Count(),
                    GrossAmount = g.Sum(r => r.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(r => r.CommissionAmount),
                    NetAmount = g.Sum(r => r.Balance),
                    Closed = g.All(r =>
                        r.CouponsSold + r.CouponsReturned + r.Annulments.Count
                        >= (r.CouponRange.EndNumber - r.CouponRange.StartNumber + 1)),
                    SalePercentage = g.Sum(r => r.CouponsSold) * 100m
                                       / g.Sum(r => r.CouponRange.EndNumber - r.CouponRange.StartNumber + 1)
                })
                .ToListAsync();

            return View(data);
        }

        // --- By City/Department ---
        [HttpGet]
        public async Task<IActionResult> ByCityDept()
        {
            ViewBag.Cities = await _context.Vendors.Select(v => v.City).Distinct().ToListAsync();
            ViewBag.Departments = await _context.Vendors.Select(v => v.Department).Distinct().ToListAsync();
            return View(new List<CityDeptReportItem>());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ByCityDept(string city, string department)
        {
            ViewBag.Cities = await _context.Vendors.Select(v => v.City).Distinct().ToListAsync();
            ViewBag.Departments = await _context.Vendors.Select(v => v.Department).Distinct().ToListAsync();

            var query = _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRange)
                .Include(r => r.Annulments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(city))
                query = query.Where(r => r.Vendor.City == city);
            if (!string.IsNullOrEmpty(department))
                query = query.Where(r => r.Vendor.Department == department);

            var data = await query
                .GroupBy(r => new { r.Vendor.Id, r.Vendor.Name })
                .Select(g => new CityDeptReportItem
                {
                    VendorName = g.Key.Name,
                    Delivered = g.Sum(r => r.CouponRange.EndNumber - r.CouponRange.StartNumber + 1),
                    Sold = g.Sum(r => r.CouponsSold),
                    Returned = g.Sum(r => r.CouponsReturned),
                    Annulled = g.SelectMany(r => r.Annulments).Count(),
                    GrossAmount = g.Sum(r => r.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(r => r.CommissionAmount),
                    NetAmount = g.Sum(r => r.Balance),
                    Closed = g.All(r =>
                        r.CouponsSold + r.CouponsReturned + r.Annulments.Count
                        >= (r.CouponRange.EndNumber - r.CouponRange.StartNumber + 1)),
                    SalePercentage = g.Sum(r => r.CouponsSold) * 100m
                                       / g.Sum(r => r.CouponRange.EndNumber - r.CouponRange.StartNumber + 1)
                })
                .ToListAsync();

            return View(data);
        }

        // --- Download Coupon Ranges (Excel) ---
        [HttpGet]
        public async Task<IActionResult> DownloadRanges()
        {
            var ranges = await _context.CouponRanges.Include(r => r.Vendor).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Rangos");
            ws.Cell(1, 1).Value = "Vendedor";
            ws.Cell(1, 2).Value = "Inicio";
            ws.Cell(1, 3).Value = "Fin";

            for (int i = 0; i < ranges.Count; i++)
            {
                var r = ranges[i];
                ws.Cell(i + 2, 1).Value = r.Vendor.Name;
                ws.Cell(i + 2, 2).Value = r.StartNumber;
                ws.Cell(i + 2, 3).Value = r.EndNumber;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "CouponRanges.xlsx");
        }

        // --- Download Coupon Ranges (PDF) ---
        [HttpGet]
        public async Task<IActionResult> ExportRangesPdf()
        {
            var ranges = await _context.CouponRanges.Include(r => r.Vendor)
                                .OrderBy(r => r.Vendor.Name)
                                .ThenBy(r => r.StartNumber)
                                .ToListAsync();

            await using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            writer.SetCloseStream(false);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());
            doc.SetMargins(20, 20, 20, 20);

            doc.Add(new Paragraph("Informe de Rangos de Cupones")
                .SetBold().SetFontSize(16).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

            var table = new Table(new float[] { 4, 2, 2 }).UseAllAvailableWidth();
            table.AddHeaderCell("Vendedor").AddHeaderCell("Inicio").AddHeaderCell("Fin");

            foreach (var r in ranges)
            {
                table.AddCell(r.Vendor.Name);
                table.AddCell(r.StartNumber.ToString());
                table.AddCell(r.EndNumber.ToString());
            }

            doc.Add(table);
            doc.Close();
            ms.Position = 0;
            return File(ms.ToArray(), "application/pdf", "CouponRanges.pdf");
        }

        // --- Real-time report view ---
        [HttpGet]
        public IActionResult RealTime()
            => View();

        // JSON for real-time
        [HttpGet]
        public async Task<JsonResult> RealTimeData()
        {
            var data = await _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRange)
                .Include(r => r.Annulments)
                .GroupBy(r => new { r.Vendor.Id, r.Vendor.Name })
                .Select(g => new TypeClassVendorReportItem
                {
                    VendorName = g.Key.Name,
                    Delivered = g.Sum(r => r.CouponRange.EndNumber - r.CouponRange.StartNumber + 1),
                    Sold = g.Sum(r => r.CouponsSold),
                    Returned = g.Sum(r => r.CouponsReturned),
                    Annulled = g.SelectMany(r => r.Annulments).Count(),
                    GrossAmount = g.Sum(r => r.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(r => r.CommissionAmount),
                    NetAmount = g.Sum(r => r.Balance),
                    Closed = g.All(r =>
                        r.CouponsSold + r.CouponsReturned + r.Annulments.Count
                        >= (r.CouponRange.EndNumber - r.CouponRange.StartNumber + 1)),
                    SalePercentage = g.Sum(r => r.CouponsSold) * 100m
                                       / g.Sum(r => r.CouponRange.EndNumber - r.CouponRange.StartNumber + 1)
                })
                .ToListAsync();

            return Json(data);
        }

        // --- Export Real-time to Excel ---
        [HttpGet]
        public async Task<IActionResult> ExportRealTimeExcel()
        {
            var items = (await RealTimeData().ConfigureAwait(false))
                        .Value as List<TypeClassVendorReportItem> ?? new();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("RealTime");
            var headers = new[] { "Vendor", "Delivered", "Sold", "Returned", "Annulled", "Gross", "Commission", "Net", "Closed", "Sale%" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                ws.Cell(i + 2, 1).Value = it.VendorName;
                ws.Cell(i + 2, 2).Value = it.Delivered;
                ws.Cell(i + 2, 3).Value = it.Sold;
                ws.Cell(i + 2, 4).Value = it.Returned;
                ws.Cell(i + 2, 5).Value = it.Annulled;
                ws.Cell(i + 2, 6).Value = it.GrossAmount;
                ws.Cell(i + 2, 7).Value = it.CommissionAmount;
                ws.Cell(i + 2, 8).Value = it.NetAmount;
                ws.Cell(i + 2, 9).Value = it.Closed;
                ws.Cell(i + 2, 10).Value = it.SalePercentage;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "RealTimeReport.xlsx");
        }

        // --- Export Real-time to PDF ---
        [HttpGet]
        public async Task<IActionResult> ExportRealTimePdf()
        {
            var items = (await RealTimeData().ConfigureAwait(false))
                        .Value as List<TypeClassVendorReportItem> ?? new();

            await using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            writer.SetCloseStream(false);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
            doc.SetMargins(20, 20, 20, 20);

            doc.Add(new Paragraph("Real-Time Report")
                .SetBold().SetFontSize(16).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

            var table = new Table(new float[] { 3, 2, 2, 2, 2, 2, 2, 2, 2, 2 }).UseAllAvailableWidth();
            foreach (var hdr in new[] { "Vendor", "Delivered", "Sold", "Returned", "Annulled", "Gross", "Commission", "Net", "Closed", "Sale%" })
                table.AddHeaderCell(hdr);

            foreach (var it in items)
            {
                table.AddCell(it.VendorName);
                table.AddCell(it.Delivered.ToString());
                table.AddCell(it.Sold.ToString());
                table.AddCell(it.Returned.ToString());
                table.AddCell(it.Annulled.ToString());
                table.AddCell(it.GrossAmount.ToString("F0"));
                table.AddCell(it.CommissionAmount.ToString("F0"));
                table.AddCell(it.NetAmount.ToString("F0"));
                table.AddCell(it.Closed ? "Yes" : "No");
                table.AddCell(it.SalePercentage.ToString("F2"));
            }

            doc.Add(table);
            doc.Close();
            ms.Position = 0;
            return File(ms.ToArray(), "application/pdf", "RealTimeReport.pdf");
        }


        // 1) Vista en HTML con tabla y botones
        public async Task<IActionResult> CouponRanges()
        {
            var data = await _context.CouponRanges
                .Include(cr => cr.Vendor)
                .Select(cr => new RangeReportItem
                {
                    VendorName = cr.Vendor.Name,
                    StartNumber = cr.StartNumber,
                    EndNumber = cr.EndNumber
                })
                .ToListAsync();

            return View(data);
        }

        // 2) Exportar PDF
        public async Task<IActionResult> ExportPdfCouponRanges()
        {
            var data = await _context.CouponRanges
                .Include(cr => cr.Vendor)
                .Select(cr => new RangeReportItem
                {
                    VendorName = cr.Vendor.Name,
                    StartNumber = cr.StartNumber,
                    EndNumber = cr.EndNumber
                })
                .ToListAsync();

            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            doc.Add(new Paragraph("Informe de Rangos de Cupones")
                .SetFontSize(14).SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            doc.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}\n\n"));

            // tabla de 3 columnas
            var table = new Table(3).UseAllAvailableWidth();
            table.AddHeaderCell("Vendedor");
            table.AddHeaderCell("Desde");
            table.AddHeaderCell("Hasta");

            foreach (var row in data)
            {
                table.AddCell(row.VendorName);
                table.AddCell(row.StartNumber.ToString());
                table.AddCell(row.EndNumber.ToString());
            }

            doc.Add(table);
            doc.Close();

            return File(ms.ToArray(),
                        "application/pdf",
                        $"Rangos_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // 3) Exportar Excel
        public async Task<IActionResult> ExportExcelCouponRanges()
        {
            var data = await _context.CouponRanges
                .Include(cr => cr.Vendor)
                .Select(cr => new RangeReportItem
                {
                    VendorName = cr.Vendor.Name,
                    StartNumber = cr.StartNumber,
                    EndNumber = cr.EndNumber
                })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Rangos");
            // encabezados
            ws.Cell(1, 1).Value = "Vendedor";
            ws.Cell(1, 2).Value = "Desde";
            ws.Cell(1, 3).Value = "Hasta";

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                ws.Cell(row, 1).Value = data[i].VendorName;
                ws.Cell(row, 2).Value = data[i].StartNumber;
                ws.Cell(row, 3).Value = data[i].EndNumber;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Rangos_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }


    // ----- DTO classes for reports -----
    //public class TypeClassVendorReportItem
    //{
    //    public string VendorName { get; set; } = "";
    //    public int Delivered { get; set; }
    //    public int Sold { get; set; }
    //    public int Returned { get; set; }
    //    public int Annulled { get; set; }
    //    public decimal GrossAmount { get; set; }
    //    public decimal CommissionAmount { get; set; }
    //    public decimal NetAmount { get; set; }
    //    public bool Closed { get; set; }
    //    public decimal SalePercentage { get; set; }
    //}

    //public class CityDeptReportItem
    //{
    //    public string VendorName { get; set; } = "";
    //    public int Delivered { get; set; }
    //    public int Sold { get; set; }
    //    public int Returned { get; set; }
    //    public int Annulled { get; set; }
    //    public decimal GrossAmount { get; set; }
    //    public decimal CommissionAmount { get; set; }
    //    public decimal NetAmount { get; set; }
    //    public bool Closed { get; set; }
    //    public decimal SalePercentage { get; set; }
    //}
}
