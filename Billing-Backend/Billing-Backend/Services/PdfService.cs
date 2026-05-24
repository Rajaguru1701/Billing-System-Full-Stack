using Billing_Backend.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Billing_Backend.Services
{
    public class PdfService
    {
        private readonly AppDbContext _context;

        // Branding Palette sampled from your traditional banner
        private readonly string BrandOrangeHex = "#D97706";
        private readonly string TableHeaderBgHex = "#FEF3C7";

        public PdfService(AppDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;

            // 🛠️ THE FIX: Dynamically register the Tamil Unicode Font files on startup
            string regularFontPath = @"C:\Users\Admin\source\repos\Billing-Backend\Billing-Backend\Assets\NotoSansTamil-Regular.ttf";
            string boldFontPath = @"C:\Users\Admin\source\repos\Billing-Backend\Billing-Backend\Assets\NotoSansTamil-Bold.ttf";

            if (File.Exists(regularFontPath))
            {
                using var stream = File.OpenRead(regularFontPath);
                FontManager.RegisterFont(stream);
            }
            if (File.Exists(boldFontPath))
            {
                using var stream = File.OpenRead(boldFontPath);
                FontManager.RegisterFont(stream);
            }
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(Guid billId)
        {
            var bill = await _context.Bills
                .Include(b => b.Customer)
                .Include(b => b.BillItems)
                    .ThenInclude(bi => bi.Item)
                .FirstOrDefaultAsync(b => b.BillId == billId);

            if (bill == null) throw new Exception("Invoice database record not found.");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);

                    // Set default fallback system font for numbers/English text
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3).FontFamily("Helvetica"));

                    // 1. THE HEADER (EDGE-TO-EDGE BANNER)
                    page.Header().Column(headerCol =>
                    {
                        string bannerPath = @"C:\Users\Admin\source\repos\Billing-Backend\Billing-Backend\Assets\Arasan.jpg";

                        if (File.Exists(bannerPath))
                        {
                            headerCol.Item().Image(bannerPath);
                        }
                        else
                        {
                            headerCol.Item().Padding(54).PaddingBottom(10).Row(row =>
                            {
                                row.RelativeColumn().Text("⚡ ARASAN HANDICRAFTS").FontSize(24).Bold().FontColor(BrandOrangeHex);
                            });
                        }

                        // Metadata Row
                        headerCol.Item().PaddingHorizontal(54).PaddingTop(15).PaddingBottom(10).Row(metaRow =>
                        {
                            metaRow.RelativeColumn().Text($"Date: {bill.BillDate:dd-MMM-yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken1);
                            metaRow.ConstantColumn(200).AlignRight().Text($"Invoice ID: {bill.BillId.ToString().Substring(0, 8).ToUpper()}").FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                        });

                        headerCol.Item().PaddingHorizontal(54).LineHorizontal(1f).LineColor(Colors.Grey.Lighten1);
                    });

                    // 2. THE CONTENT
                    page.Content().PaddingHorizontal(54).PaddingVertical(20).Column(col =>
                    {
                        col.Item().Text("Client Details").Bold().Underline();
                        col.Item().Text($"Name: {bill.Customer?.Name}");
                        col.Item().Text($"Phone: {bill.Customer?.PhoneNo}");
                        col.Item().Text($"Address: {bill.Customer?.Address}");
                        col.Item().PaddingBottom(20);

                        // Table Grid
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(TableHeaderBgHex).Padding(5).Text("Product Description").Bold().FontColor(BrandOrangeHex);
                                header.Cell().Background(TableHeaderBgHex).Padding(5).Text("Unit Price").Bold().FontColor(BrandOrangeHex);
                                header.Cell().Background(TableHeaderBgHex).Padding(5).Text("Qty").Bold().FontColor(BrandOrangeHex);
                                header.Cell().Background(TableHeaderBgHex).Padding(5).Text("Total").Bold().FontColor(BrandOrangeHex);
                            });

                            foreach (var lineItem in bill.BillItems)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(lineItem.Item?.ItemName ?? "Unknown Item");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{lineItem.Price:F2}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(lineItem.Quantity.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{lineItem.Total:F2}");
                            }
                        });

                        col.Item().AlignRight().PaddingTop(20).Column(totalCol =>
                        {
                            totalCol.Item().Text($"GST Tax Accrued: {bill.GstAmount:F2}");
                            totalCol.Item().Text($"Grand Total Due: {bill.TotalAmount:F2}").FontSize(14).Bold().FontColor(BrandOrangeHex);
                        });
                    });

                    // 3. THE FOOTER (TRADITIONAL TAGLINE & MADURAI ADDRESS)
                    page.Footer().PaddingHorizontal(54).PaddingBottom(20).Column(footerCol =>
                    {
                        footerCol.Item().PaddingBottom(8).LineHorizontal(1.5f).LineColor(BrandOrangeHex);

                        // Clean Tamil Tagline rendering flawlessly via Noto Sans Tamil 
                        footerCol.Item().AlignCenter().Text("தரம் எங்களின் தனித்துவம்")
                            .FontFamily("Noto Sans Tamil") // 👈 FIXED: Changed from FontName to FontFamily
                            .FontSize(12)
                            .Bold()
                            .FontColor(BrandOrangeHex);

                        // Address Row
                        footerCol.Item().PaddingTop(2).AlignCenter().Text("7, pandiya vellalar street, 1st lane, madurai - 625001.")
                            .FontFamily("Helvetica") // 👈 FIXED: Changed from FontName to FontFamily
                            .FontSize(10)
                            .FontColor(BrandOrangeHex);

                        footerCol.Item().PaddingTop(10).AlignCenter().Text(x =>
                        {
                            x.Span("Page ").FontSize(9).FontColor(Colors.Grey.Medium);
                            x.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                            x.Span(" / ").FontSize(9).FontColor(Colors.Grey.Medium);
                            x.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }
    }
}