using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RifaDeliverySystem.Web.Data;
using RifaDeliverySystem.Web.Models;
using RifaDeliverySystem.Web.Services.Pdf;
using System;
using System.Linq;
using System.Threading.Tasks;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace RifaDeliverySystem.Web.Controllers
{
 

// Modelo de proyección para exportar
public class CouponRangeExportItem
    {
        public int Id { get; set; }
        public string VendorName { get; set; } = "";
        public int StartNumber { get; set; }
        public int EndNumber { get; set; }
        public int Count => EndNumber - StartNumber + 1;
        public bool Asignado => RenditionId != null;
        public int? RenditionId { get; set; }
        public DateTime? CreatedAt { get; set; }  // si existe en tu modelo
    }

    public partial class CouponRangesController : Controller
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
    


        private async Task<List<CouponRangeExportItem>> GetCouponRangesForExportAsync(
            int? vendorId,
            bool? onlyAvailable,
            string? search
        // agrega más filtros si tu Index los usa
        )
        {
            var q = _context.CouponRanges
                .Include(x => x.Vendor)
                .AsQueryable();

            if (vendorId.HasValue)
                q = q.Where(x => x.VendorId == vendorId.Value);

            if (onlyAvailable == true)
                q = q.Where(x => x.RenditionId == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                // ejemplo: buscar por vendor o por rango
                q = q.Where(x =>
                    x.Vendor.Name.Contains(search) ||
                    EF.Functions.ILike(x.Vendor.Name, $"%{search}%") ||
                    x.StartNumber.ToString().Contains(search) ||
                    x.EndNumber.ToString().Contains(search));
            }

            // mismo orden que muestres en el Index
            q = q.OrderBy(x => x.Vendor.Name).ThenBy(x => x.StartNumber);

            return await q.Select(x => new CouponRangeExportItem
            {
                Id = x.Id,
                VendorName = x.Vendor.Name,
                StartNumber = x.StartNumber,
                EndNumber = x.EndNumber,
                RenditionId = x.RenditionId,
                // CreatedAt = x.CreatedAt // si lo tenés
            }).ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> ExportIndexExcel(
            int? vendorId,
            bool? onlyAvailable,
            string? search
        )
        {
            var data = await GetCouponRangesForExportAsync(vendorId, onlyAvailable, search);

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("CouponRanges");
            int row = 1;

            // Encabezados
            ws.Cell(row, 1).Value = "ID";
            ws.Cell(row, 2).Value = "Vendedor";
            ws.Cell(row, 3).Value = "Inicio";
            ws.Cell(row, 4).Value = "Fin";
            ws.Cell(row, 5).Value = "Cantidad";
            ws.Cell(row, 6).Value = "Asignado";
            ws.Cell(row, 7).Value = "RenditionId";
            ws.Range(row, 1, row, 7).Style.Font.Bold = true;

            // Filas
            foreach (var i in data)
            {
                row++;
                ws.Cell(row, 1).Value = i.Id;
                ws.Cell(row, 2).Value = i.VendorName;
                ws.Cell(row, 3).Value = i.StartNumber;
                ws.Cell(row, 4).Value = i.EndNumber;
                ws.Cell(row, 5).Value = i.Count;
                ws.Cell(row, 6).Value = i.Asignado ? "Sí" : "No";
                ws.Cell(row, 7).Value = i.RenditionId;
            }

            // Totales
            row++;
            ws.Cell(row, 1).Value = "Totales:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 5).FormulaA1 = $"SUM(E2:E{row - 1})";
            ws.Cell(row, 5).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var fileName = $"CouponRanges_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportIndexPdf(
            int? vendorId,
            bool? onlyAvailable,
            string? search
        )
        {
            var data = await GetCouponRangesForExportAsync(vendorId, onlyAvailable, search);
            var culture = new System.Globalization.CultureInfo("es-PY");

            byte[] pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.Header().Text("Listado de Rango de Cupones").SemiBold().FontSize(16);
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();   // ID
                            cols.RelativeColumn(3);  // Vendedor
                            cols.RelativeColumn();   // Inicio
                            cols.RelativeColumn();   // Fin
                            cols.RelativeColumn();   // Cantidad
                            cols.RelativeColumn();   // Asignado
                            cols.RelativeColumn();   // RenditionId
                        });

                        void H(string t) => table.Cell().Element(h => h
                            .Padding(4).Background(Colors.Grey.Lighten3)
                            .BorderBottom(1).DefaultTextStyle(x => x.SemiBold()))
                            .Text(t);

                        H("ID"); H("Vendedor"); H("Inicio"); H("Fin"); H("Cantidad"); H("Asignado"); H("RenditionId");

                        foreach (var i in data)
                        {
                            table.Cell().Padding(2).Text(i.Id.ToString());
                            table.Cell().Padding(2).Text(i.VendorName);
                            table.Cell().Padding(2).AlignRight().Text(i.StartNumber.ToString("N0", culture));
                            table.Cell().Padding(2).AlignRight().Text(i.EndNumber.ToString("N0", culture));
                            table.Cell().Padding(2).AlignRight().Text(i.Count.ToString("N0", culture));
                            table.Cell().Padding(2).AlignCenter().Text(i.Asignado ? "Sí" : "No");
                            table.Cell().Padding(2).AlignRight().Text(i.RenditionId?.ToString() ?? "");
                        }

                        // Totales
                        var totalCount = data.Sum(x => x.Count);
                        table.Cell().Padding(4).Background(Colors.Grey.Lighten4).BorderTop(1).Text("Totales").SemiBold();
                        table.Cell().Padding(4).Background(Colors.Grey.Lighten4).BorderTop(1).Text(""); // vendedor
                        table.Cell().Padding(4).Background(Colors.Grey.Lighten4).BorderTop(1).Text(""); // inicio
                        table.Cell().Padding(4).Background(Colors.Grey.Lighten4).BorderTop(1).Text(""); // fin
                        table.Cell().Padding(4).Background(Colors.Grey.Lighten4).BorderTop(1).AlignRight().Text(totalCount.ToString("N0", culture)).SemiBold();
                        table.Cell().Padding(4).Background(Colors.Grey.Lighten4).BorderTop(1).Text(""); // asignado
                        table.Cell().Padding(4).Background(Colors.Grey.Lighten4).BorderTop(1).Text(""); // renditionId
                    });

                    page.Footer().AlignRight().Text(txt =>
                    {
                        txt.Span("Generado: ");
                        txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                });
            }).GeneratePdf();

            var fileName = $"CouponRanges_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdf, "application/pdf", fileName);
        }
    }


}
