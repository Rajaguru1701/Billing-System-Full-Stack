using Billing_Backend.Data;
using Billing_Backend.Dto;
using Billing_Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Billing_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        private readonly BillService _billService;
        private readonly PdfService _pdfService;
        private readonly AppDbContext _context;

        public BillingController(BillService billService,PdfService pdfService,AppDbContext context)
        {
            _billService = billService;
            _pdfService = pdfService;
            _context = context;

        }

        [HttpGet("download-pdf/{billId}")]
        public async Task<IActionResult> DownloadInvoicePdf(Guid billId)
        {
            try
            {
                byte[] pdfBytes = await _pdfService.GenerateInvoicePdfAsync(billId);

                // Return raw binary asset as an attachment download trigger file block
                string fileName = $"Invoice_{billId.ToString().Substring(0, 8)}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = $"Error compiling PDF export: {ex.Message}" });
            }
        }

        [HttpPost("save-bill")]
        public async Task<IActionResult> SaveBill([FromBody] BillDto billDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var savedBill = await _billService.SaveBillAsync(billDto);

                // Return generated operational IDs back to frontend context tracking
                return Ok(new
                {
                    message = "Multi-item invoice saved successfully!",
                    billId = savedBill.BillId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database persistence error: {ex.Message}");
            }
        }

        [HttpPost("history")]
        public async Task<IActionResult> GetBillHistory([FromBody] BillSearchDto searchParams)
        {
            // Start with a base query tracking the main tables
            var query = _context.Bills
                .Include(b => b.Customer)
                .OrderByDescending(b => b.BillDate) // Newest bills first!
                .AsQueryable();

            // 1. Filter by Customer Name (Case-insensitive match)
            if (!string.IsNullOrWhiteSpace(searchParams.CustomerName))
            {
                query = query.Where(b => b.Customer != null &&
                    b.Customer.Name.ToLower().Contains(searchParams.CustomerName.ToLower()));
            }

            // 2. Filter by Phone Number
            if (!string.IsNullOrWhiteSpace(searchParams.PhoneNo))
            {
                query = query.Where(b => b.Customer != null &&
                    b.Customer.PhoneNo.Contains(searchParams.PhoneNo));
            }

            // 3. Filter by Date Intervals
            if (searchParams.FromDate.HasValue)
            {
                // Set to start of the day (00:00:00)
                var fromDate = searchParams.FromDate.Value.Date;
                query = query.Where(b => b.BillDate >= fromDate);
            }

            if (searchParams.ToDate.HasValue)
            {
                // Set to end of the day (23:59:59) to capture everything on that day
                var toDate = searchParams.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(b => b.BillDate <= toDate);
            }

            // Execute the query cleanly and map it to a lightweight response layout
            var results = await query.Select(b => new
            {
                b.BillId,
                b.BillDate,
                CustomerName = b.Customer != null ? b.Customer.Name : "Walk-in Customer",
                CustomerPhone = b.Customer != null ? b.Customer.PhoneNo : "N/A",
                b.GstAmount,
                b.TotalAmount
            }).ToListAsync();

            return Ok(results);
        }
    }
}
