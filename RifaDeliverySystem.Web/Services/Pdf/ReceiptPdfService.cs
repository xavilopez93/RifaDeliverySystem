// Services/Pdf/ReceiptPdfService.cs
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using iText.Commons.Actions.Contexts;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RifaDeliverySystem.Web.Areas;
using RifaDeliverySystem.Web.Data;      // tu DbContext
using RifaDeliverySystem.Web.Extensions;
using RifaDeliverySystem.Web.Models;   // Rendition, Vendor…
using System.Globalization;
using Cell = iText.Layout.Element.Cell;
using Table = iText.Layout.Element.Table;

namespace RifaDeliverySystem.Web.Services.Pdf
{
    public interface IReceiptPdfService
    {
        Task<byte[]> GenerateAsync(int renditionId);
    }

    public sealed class ReceiptPdfService : IReceiptPdfService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ReceiptPdfService(IWebHostEnvironment env, ApplicationDbContext ctx, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _ctx = ctx;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<byte[]> GenerateAsync(int renditionId)
        {
            // 1) Traemos la rendición con sus dependencias REALES
            var r = await _ctx.Renditions
                              .Include(x => x.Vendor)
                              .Include(x => x.CouponRanges)  // Rango de cupones (puede ser varios)
                                                             //    .Include(x => x.Annulments)
                              .Include(x => x.Payments)
                              .FirstOrDefaultAsync(x => x.Id == renditionId);
            if (r is null)
                throw new KeyNotFoundException("Rendición no encontrada");

            // 2) Cálculos que pide el comprobante
            int deliveredQty = r.CouponRanges.OfType<CouponRange>().Sum(cr => cr.EndNumber - cr.StartNumber + 1);
            
            int soldQty = r.CouponsSold;
           // int annulledQty = r.Annulments.Count;
            int returnedQty = r.CouponsReturned;
            int lostQty = r.Extravio;   // no hay propiedad => 0
            int stolenQty = r.Robo;

            decimal grossGs = soldQty * 10_000m;          // Precio base por cupón
            decimal commissionGs = r.CommissionAmount;
            decimal commissionPct = grossGs == 0 ? 0 : Math.Round(commissionGs * 100 / grossGs, 2);
            decimal netGs = grossGs - commissionGs;

            var httpUser = _httpContextAccessor.HttpContext?.User;
            var user = await _userManager.GetUserAsync(httpUser);
            var verificadorNombre = user?.Email ?? user?.UserName ?? "Verificador desconocido";
            // Métodos de pago (hoy es solo lista de strings)
            var payments = r.Payments.OrderBy(p => p.Type).ToList();

            var ranges = await _ctx.CouponRanges
       .Where(cr => r.RangeIds.Contains(cr.Id))
       .ToListAsync();

            // 3) Construimos el PDF
            using var ms = new MemoryStream();
            using var pdf = new PdfDocument(new PdfWriter(ms));
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(30, 20, 30, 20);

            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // --- Encabezado simple ---
            doc.Add(new Paragraph($"Rendición N.º {r.Id}")            // usa Id como correlativo
                    .SetFont(bold).SetFontSize(12));
            doc.Add(new Paragraph($"Fecha: {r.Date:dd/MM/yyyy}").SetFont(font));

            doc.Add(new Paragraph($"Vendedor: {r.Vendor.Name}")       // ajusta .Name si diferente
                    .SetFont(font));

            doc.Add(new Paragraph($"Verificador: {verificadorNombre}")
         .SetFont(font).SetFontSize(10));
            doc.Add(new Paragraph(" ").SetFont(font)); // separador

            // --- Detalle de cupones ---
            var detail = new Table(UnitValue.CreatePercentArray(new[] { 70f, 30f }))
                            .UseAllAvailableWidth();
            AddRow("Cupones entregados", deliveredQty, detail);
            AddRow("Cupones vendidos", soldQty, detail);
            //AddRow("Cupones anulados", annulledQty, detail);
            AddRow("Cupones extraviados", lostQty, detail);
            AddRow("Cupones robados", stolenQty, detail);
            AddRow("Cupones retornados", returnedQty, detail);
            doc.Add(detail);

            doc.Add(new Paragraph(" ").SetFont(font));

            // --- Cálculo de dinero ---
            var money = new Table(UnitValue.CreatePercentArray(new[] { 60f, 40f }))
                           .UseAllAvailableWidth();
            money.AddCell("Total vendido (Gs.)");
            money.AddCell($"{grossGs:N0}").SetTextAlignment(TextAlignment.RIGHT);
            money.AddCell($"Comisión {commissionPct:N2}%");
            money.AddCell($"-{commissionGs:N0}").SetTextAlignment(TextAlignment.RIGHT);
            money.AddCell(new Cell(1, 2)
                .Add(new Paragraph($"Monto neto recibido: {netGs:N0}")
                     .SetFont(bold)).SetTextAlignment(TextAlignment.RIGHT));
            doc.Add(money);

            // --- Métodos de pago (solo nombres) ---
            doc.Add(new Paragraph("Formas de Cobro:").SetFont(bold));
            //foreach (var p in payments)
            //    doc.Add(new Paragraph($"• {p}").SetFont(font));
            var payTable = new Table(UnitValue.CreatePercentArray(new[] { 30f, 25f, 45f }))
                .UseAllAvailableWidth();
            payTable.AddCell("Forma").SetFont(bold)
                    .AddCell("Monto (Gs.)").SetFont(bold)
                    .AddCell("N.º Comprobante").SetFont(bold);
            foreach (var p in payments)
            {
                payTable.AddCell(p.Type.GetDisplayName()).SetFont(bold);
                payTable.AddCell($"{p.Amount:N0}").SetFont(bold).SetTextAlignment(TextAlignment.RIGHT);
                payTable.AddCell(p.ReceiptNumber ?? "-").SetFont(bold);
            }
            doc.Add(payTable);
            // Calcular el total de pagos realizados. Si existen pagos registrados, el monto total en letras
            // corresponde a la suma de esos pagos; de lo contrario, usar el neto calculado previamente (ventas - comisión).
            var totalPayments = payments.Sum(p => p.Amount);
            var netInGs = totalPayments != 0m ? totalPayments : netGs;

            doc.Add(new Paragraph($"Monto total en letras: {NumeroALetras((long)netInGs)}").SetFontSize(9));

            // --- Firmas ---
            var firmas = new Table(UnitValue.CreatePercentArray(new[] { 50f, 50f }))
                            .UseAllAvailableWidth();
            firmas.AddCell(Firma("FIRMA DEL VERIFICADOR"));
            firmas.AddCell(Firma("FIRMA DEL VENDEDOR"));
            doc.Add(firmas);

            doc.Close();
            return ms.ToArray();

            // ---------- helpers locales ----------
            void AddRow(string label, int qty, Table t)
            {
                t.AddCell(new Paragraph(label).SetFont(font));
                t.AddCell(new Paragraph($"{qty:N0} unidades")
                           .SetFont(bold)
                           .SetTextAlignment(TextAlignment.RIGHT));
            }

            static Cell Firma(string titulo) =>
                new Cell()
                    .Add(new Paragraph(titulo).SetFontSize(9))
                    .Add(new Paragraph(" ").SetMinHeight(25))
                    .SetTextAlignment(TextAlignment.CENTER);
        }

        private static string NumeroALetras(long numero)
        {
            // Implementa tu rutina propia o usa Humanizer
            return numero.ToString("N0", CultureInfo.GetCultureInfo("es-PY"));
        }
    }
}
