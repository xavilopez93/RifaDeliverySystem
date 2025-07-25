using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Models;
using RifaDeliverySystem.Web.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        // Fix for CS0103: The name 'SalePercentage' does not exist in the current context
        // The issue occurs because the calculation for `SalePercentage` is incomplete and references an undefined property or method.
        // The fix involves completing the calculation by using the correct property or method for the denominator.

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ByTypeClassVendor(string type, string vendorClass)
        {
            ViewBag.Types = await _context.CommissionRules.Select(r => r.VendorType).Distinct().ToListAsync();
            ViewBag.Classes = await _context.CommissionRules.Select(r => r.VendorClass).Distinct().ToListAsync();

            var query = _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRanges)
               // .Include(r => r.Annulments)
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
                    Sold = g.Sum(r => r.CouponsSold),
                    Returned = g.Sum(r => r.CouponsReturned),
                    Annulled = g.Sum(r => r.Extravio + r.Robo),
                    GrossAmount = g.Sum(r => r.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(r => r.CommissionAmount),
                    NetAmount = g.Sum(r => r.Balance),
                    Closed = g.All(r =>
                        r.CouponsSold + r.CouponsReturned + r.Extravio + r.Robo
                        >= (r.CouponRanges.Sum(cr => cr.EndNumber - cr.StartNumber + 1))),
                    SalePercentage = g.Sum(r => r.CouponsSold) * 100m
                                      / g.Sum(r => r.CouponRanges.Sum(cr => cr.EndNumber - cr.StartNumber + 1)) // Corrected denominator
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
                .Include(r => r.CouponRanges)
                //.Include(r => r.Annulments)
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
                    Delivered = g.Sum(r => r.CouponsSold),
                    Sold = g.Sum(r => r.CouponsSold),
                    Returned = g.Sum(r => r.CouponsReturned),
                    Annulled = g.Sum(r => r.Extravio + r.Robo),
                    GrossAmount = g.Sum(r => r.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(r => r.CommissionAmount),
                    NetAmount = g.Sum(r => r.Balance),
                    Closed = g.All(r =>
                        r.CouponsSold + r.CouponsReturned + r.Extravio + r.Robo
>= (r.CouponRanges.Sum(cr => cr.EndNumber - cr.StartNumber + 1))),
                    SalePercentage = g.Sum(r => r.CouponsSold) * 100m
                                       / g.Sum(r => r.CouponRanges.Sum(cr => cr.EndNumber - cr.StartNumber + 1))
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
        //public async Task<IActionResult> RealTime()
        //{
        //    var data = await _context.Renditions
        //        //.Include(r => r.CouponRanges)
        //        .Include(r => r.Vendor)
        //        .GroupBy(r => new { r.Vendor.Name })
        //        .Select(g => new RealTimeReportItem
        //        {
        //            //BranchName = g.Key.Name,
        //            SellerName = g.Key.Name,
        //            CouponCount = g.Sum(r => r.CouponRanges.Count),
        //            TotalAmount = g.Sum(r => r.CouponRanges.Count * 10000)
        //        })
        //        .ToListAsync();

        //    return View(data);
        //}


        // JSON for real-time
        [HttpGet]
        public async Task<IActionResult> RealTimeData()
        {
            var data = await _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRanges)
               // .Include(r => r.Annulments)
                .GroupBy(r => new { r.Vendor.Id, r.Vendor.Name })
                .Select(g => new TypeClassVendorReportItem
                {
                    VendorName = g.Key.Name,
                    Delivered = g.Sum(r => r.CouponRanges.Sum(cr => cr.EndNumber - cr.StartNumber + 1)),
                    Sold = g.Sum(r => r.CouponsSold),
                    Returned = g.Sum(r => r.CouponsReturned),
                    Annulled = g.Sum(r => r.Extravio + r.Robo),
                    GrossAmount = g.Sum(r => r.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(r => r.CommissionAmount),
                    NetAmount = g.Sum(r => r.Balance),
                    Closed = g.All(r =>
                        r.CouponsSold + r.CouponsReturned + r.Extravio + r.Robo
>= (r.CouponRanges.Sum(cr => cr.EndNumber - cr.StartNumber + 1))),
                    SalePercentage = g.Sum(r => r.CouponsSold) * 100m
                                       / g.Sum(r => r.CouponRanges.Sum(cr => cr.EndNumber - cr.StartNumber + 1))
                })
                .ToListAsync();

            return View(data);
        }

        // --- Export Real-time to Excel ---
        [HttpGet]
        public async Task<IActionResult> ExportRealTimeExcel()
        {
            var result = await RealTimeData().ConfigureAwait(false);
            var items = result is JsonResult jsonResult && jsonResult.Value is List<TypeClassVendorReportItem> data
                ? data
                : new List<TypeClassVendorReportItem>();

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
            var result = await RealTimeData().ConfigureAwait(false);
            var items = result is JsonResult jsonResult && jsonResult.Value is List<TypeClassVendorReportItem> data
                ? data
                : new List<TypeClassVendorReportItem>();

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



        // Fix for CS1061: 'ApplicationDbContext' does not contain a definition for 'Models'
        // The issue occurs because the code is trying to access a non-existent 'Models' property on the ApplicationDbContext class.
        // The fix involves removing the incorrect reference to 'Models' and directly accessing the correct DbSet property.

        //public async Task<IActionResult> BySellerBreakdown()
        //{
           
        //}


        public async Task<IActionResult> ExportSummaryByVendor()
        {
            var data = await _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRanges)
                .Where(r => r.VendorId != null)
                .GroupBy(r => new
                {
                    r.Vendor.Class,
                    r.Vendor.Type,
                    r.Vendor.Name,
                    r.CommissionAmount
                })
                .Select(g => new DeliverySalesReportItem
                {
                    SellerCategory = g.Key.Class.ToString(),
                    SellerType = g.Key.Type.ToString(),
                    SellerName = g.Key.Name,

                    DeliveredCoupons = g.Sum(t => t.CouponsReturned),
                    SoldCoupons = g.Sum(t => t.CouponsSold),
                    GrossAmount = g.Sum(t => t.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(t => t.CommissionAmount),
              
             
                })
                .ToListAsync();

            foreach (var item in data)
                item.NetAmount = item.GrossAmount - item.CommissionAmount;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Resumen por Vendedor");

            // Cabecera
            worksheet.Cell(1, 1).Value = "Tipo";
            worksheet.Cell(1, 2).Value = "Clase";
            worksheet.Cell(1, 3).Value = "Vendedor";
            worksheet.Cell(1, 4).Value = "Retornados";
            worksheet.Cell(1, 5).Value = "Vendidos";
            worksheet.Cell(1, 6).Value = "Monto Bruto";
            worksheet.Cell(1, 7).Value = "Comisi�n";
            worksheet.Cell(1, 8).Value = "Monto Neto";

            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.SellerCategory;
                worksheet.Cell(row, 2).Value = item.SellerType;
                worksheet.Cell(row, 3).Value = item.SellerName;
                worksheet.Cell(row, 4).Value = item.DeliveredCoupons;
                worksheet.Cell(row, 5).Value = item.SoldCoupons;
                worksheet.Cell(row, 6).Value = item.GrossAmount;
                worksheet.Cell(row, 7).Value = item.CommissionAmount;
                worksheet.Cell(row, 8).Value = item.NetAmount;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       $"ResumenVendedores_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        [HttpGet]
        //public async Task<IActionResult> SummaryByVendor()
        //{
        //    var filter = new VendorSummaryFilter
        //    {
        //        StartDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-30), DateTimeKind.Utc),
        //        EndDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        //    };

        //    // Dropdown de Sucursales
        //    var branches = await _context.
        //        .Select(b => new { b.Id, b.Name })
        //        .ToListAsync();
        //    ViewBag.Branches = new SelectList(branches, "Id", "Name");

        //    // Dropdown de Tipos de Vendedor
        //    var vendorTypes = await _context.Vendors
        //        .Select(v => v.Type)
        //        .Distinct()
        //        .ToListAsync();
        //    ViewBag.VendorTypes = new SelectList(vendorTypes.Select(t => new { Type = t }), "Type", "Type");

        //    // Dropdown de Clases de Vendedor
        //    var vendorClasses = await _ctx.Vendors
        //        .Select(v => v.Class)
        //        .Distinct()
        //        .ToListAsync();
        //    ViewBag.VendorClasses = new SelectList(vendorClasses.Select(c => new { Class = c }), "Class", "Class");

        //    return View(filter);
        //}

        public async Task<IActionResult> SummaryByVendor()
        {
            //var model = new VendorSummaryFilter
            //{
            //    TypeOptions = Enum.GetValues(typeof(Vendor))
            //        .Cast<Vendor>()
            //        .Select(t => new SelectListItem { Value = (t.Type).ToString(), Text = t.Type.ToString() }),

            //    ClassOptions = Enum.GetValues(typeof(VendorCategory))
            //        .Cast<VendorCategory>()
            //        .Select(c => new SelectListItem { Value = (c.Id).ToString(), Text = c.DisplayName.ToString() })

            //};


            //var filter = new VendorSummaryFilter
            //{
            //    // Establece fechas por defecto como UTC si se desea un rango predeterminado (�ltimos 30 d�as por ejemplo)
            //    StartDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-30), DateTimeKind.Utc),
            //    EndDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            //};




            // Corrected the return type to pass the list of DeliverySalesReportItem to the view
            var data = await _context.Renditions
                .Include(t => t.Vendor)
                .Where(t => t.VendorId != null)
                .GroupBy(t => new
                {
                    t.Vendor.Type,
                    t.Vendor.Class,
                    t.Vendor.Name
                })
                .Select(g => new DeliverySalesReportItem
                {
                    SellerType = g.Key.Type,
                    SellerCategory = g.Key.Class,
                    SellerName = g.Key.Name,
                    DeliveredCoupons = g.Count(),
                    SoldCoupons = g.Sum(t => t.CouponsSold),
                    GrossAmount = g.Sum(t => t.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(t => t.CommissionAmount)
                })
                .ToListAsync();

            // Calculate net amount
            foreach (var item in data)
                item.NetAmount = item.GrossAmount - item.CommissionAmount;

            // Pass the list to the view instead of attempting to cast it to a single item
            return View(data);
            //return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SummaryByVendor(VendorSummaryFilter filter)
        {
            // Asegurar que las fechas sean UTC
            if (filter.StartDate.HasValue)
                filter.StartDate = DateTime.SpecifyKind(filter.StartDate.Value, DateTimeKind.Utc);

            if (filter.EndDate.HasValue)
                filter.EndDate = DateTime.SpecifyKind(filter.EndDate.Value, DateTimeKind.Utc);

            var query = _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRanges)
                .AsQueryable();

            if (filter.StartDate != null)
                query = query.Where(r => r.Date >= filter.StartDate.Value);

            if (filter.EndDate != null)
                query = query.Where(r => r.Date <= filter.EndDate.Value);

            //if (filter.SelectedClass != null)
            //    query = query.Where(r => r.r == filter.SelectedBranchId);

            if (filter.SelectedType != null)
                query = query.Where(r => r.Vendor.Type == filter.SelectedType.Type);

            if (filter.SelectedClass != null)
                query = query.Where(r => r.Vendor.Class == filter.SelectedClass.Class);

            var data = await query
                .GroupBy(r => new
                {
                    r.Vendor.Class,
                    r.Vendor.Type,
                    r.Vendor.Name
                })
                .Select(g => new DeliverySalesReportItem
                {
                    SellerCategory = g.Key.Class.ToString(),
                    SellerType = g.Key.Type.ToString(),
                    SellerName = g.Key.Name,
                    DeliveredCoupons = g.SelectMany(r => r.CouponRanges).Count(),
                    SoldCoupons = g.Sum(t => t.CouponsSold),
                    GrossAmount = g.Sum(t => t.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(t => t.CommissionAmount)
                })
                .ToListAsync();

            return View("SummaryByVendor", data);
        }

        //public async Task<IActionResult> SummaryByVendor(VendorSummaryFilter filter)
        //{
        //    var query = _context.Renditions
        //        .Include(r => r.Vendor)
        //        .Include(r => r.CouponRanges)
        //        .AsQueryable();

        //    if (filter.StartDate.HasValue)
        //        query = query.Where(r => r.Date >= filter.StartDate.Value.ToUniversalTime());

        //    if (filter.EndDate.HasValue)
        //        query = query.Where(r => r.Date <= filter.EndDate.Value.ToUniversalTime());

        //    if (filter.SelectedType != null)
        //        query = query.Where(r => r.Vendor.Type == filter.SelectedType.Type);

        //    if (filter.SelectedClass != null)
        //        query = query.Where(r => r.Vendor.Class == filter.SelectedClass.Class);

        //    var data = await query
        //        .GroupBy(r => new
        //        {
        //            r.Vendor.Class,
        //            r.Vendor.Type,
        //            r.Vendor.Name
        //        })
        //        .Select(g => new DeliverySalesReportItem
        //        {
        //            SellerCategory = g.Key.Class.ToString(),
        //            SellerType = g.Key.Type.ToString(),
        //            SellerName = g.Key.Name,
        //            DeliveredCoupons = g.SelectMany(r => r.CouponRanges).Count(),
        //            SoldCoupons = g.Sum(t => t.CouponsSold),
        //            GrossAmount = g.Sum(t => t.CouponsSold * 10000m),
        //            CommissionAmount = g.Sum(t => t.CommissionAmount),
        //        })
        //        .ToListAsync();

        //    foreach (var item in data)
        //        item.NetAmount = item.GrossAmount - item.CommissionAmount;

        //    //filter.Results = data; // Fix: Ensure `filter.Results` is of type `IEnumerable<DeliverySalesReportItem>`.

        //    // Refill dropdown options
        //    filter.TypeOptions = Enum.GetValues(typeof(Vendor))
        //        .Cast<Vendor>()
        //        .Select(t => new SelectListItem { Value = (t.Type).ToString(), Text = t.Type.ToString() });

        //    filter.ClassOptions = Enum.GetValues(typeof(VendorCategory))
        //        .Cast<VendorCategory>()
        //        .Select(c => new SelectListItem { Value = (c.Id).ToString(), Text = c.DisplayName.ToString() });

        //    return View(filter);
        //}

        [HttpGet]
        public async Task<IActionResult> ExportSummaryByVendorPdf([FromQuery] VendorSummaryFilter filter)
        {
            var query = _context.Renditions
                .Include(r => r.Vendor)
                .Include(r => r.CouponRanges)
                .AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(r => r.Date >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.Date <= filter.EndDate.Value);

            if (filter.SelectedType != null)
                query = query.Where(r => r.Vendor.Type == filter.SelectedType.Type);

            if (filter.SelectedClass != null)
                query = query.Where(r => r.Vendor.Class == filter.SelectedClass.Class);

            var data = await query
                .GroupBy(r => new
                {
                    r.Vendor.Class,
                    r.Vendor.Type,
                    r.Vendor.Name
                })
                .Select(g => new DeliverySalesReportItem
                {
                    SellerCategory = g.Key.Class.ToString(),
                    SellerType = g.Key.Type.ToString(),
                    SellerName = g.Key.Name,
                    DeliveredCoupons = g.SelectMany(r => r.CouponRanges).Count(),
                    SoldCoupons = g.Sum(t => t.CouponsSold),
                    GrossAmount = g.Sum(t => t.CouponsSold * 10000m),
                    CommissionAmount = g.Sum(t => t.CommissionAmount),
                })
                .ToListAsync();

            foreach (var item in data)
                item.NetAmount = item.GrossAmount - item.CommissionAmount;

            using var stream = new MemoryStream();
            using var writer = new iText.Kernel.Pdf.PdfWriter(stream);
            using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var document = new iText.Layout.Document(pdf);

            document.Add(new iText.Layout.Element.Paragraph("Resumen por Vendedor").SetBold().SetFontSize(16));

            var table = new iText.Layout.Element.Table(8).UseAllAvailableWidth();
            table.AddHeaderCell("Tipo");
            table.AddHeaderCell("Clase");
            table.AddHeaderCell("Vendedor");
            table.AddHeaderCell("Entregados");
            table.AddHeaderCell("Vendidos");
            table.AddHeaderCell("Bruto");
            table.AddHeaderCell("Comisi�n");
            table.AddHeaderCell("Neto");

            foreach (var item in data)
            {
                table.AddCell(item.SellerCategory);
                table.AddCell(item.SellerType);
                table.AddCell(item.SellerName);
                table.AddCell(item.DeliveredCoupons.ToString());
                table.AddCell(item.SoldCoupons.ToString());
                table.AddCell(item.GrossAmount.ToString("N0"));
                table.AddCell(item.CommissionAmount.ToString("N0"));
                table.AddCell(item.NetAmount.ToString("N0"));
            }

            document.Add(table);
            document.Close();

            var fileName = $"ResumenVendedores_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(stream.ToArray(), "application/pdf", fileName);
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
    // Fix for CS0229: Ambiguity between 'DeliverySalesReportItem.SellerType' and 'DeliverySalesReportItem.SellerType'
    // The issue occurs because the `DeliverySalesReportItem` class has duplicate properties with the same name.
    // The fix involves removing one of the duplicate properties from the `DeliverySalesReportItem` class.

    //public class DeliverySalesReportItem
    //{
    //    public DateTime? StartDate { get; set; }
    //    public DateTime? EndDate { get; set; }
    //    public string SelectedType { get; set; }
    //    public string SelectedClass { get; set; }
    //    public IEnumerable<SelectListItem> TypeOptions { get; set; }
    //    public IEnumerable<SelectListItem> ClassOptions { get; set; }
    //    public IEnumerable<DeliverySalesReportItem> DeliverySalesReportItems { get; set; }
    //    public string SellerType { get; set; } // Retain this property
    //    public string SellerCategory { get; set; }
    //    public string SellerName { get; set; }
    //    public int DeliveredCoupons { get; set; }
    //    public int SoldCoupons { get; set; }
    //    public decimal GrossAmount { get; set; }
    //    public decimal CommissionAmount { get; set; }
    //    public decimal NetAmount { get; set; }
    //}
}