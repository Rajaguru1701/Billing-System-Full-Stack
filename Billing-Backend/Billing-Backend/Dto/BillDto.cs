using Billing_Backend.Entity;
using System.ComponentModel.DataAnnotations;

namespace Billing_Backend.Dto
{
    public class BillDto
    {
        public Guid CustomerId { get; set; }
        public string? PhoneNo { get; set; }
        public string? Address { get; set; }
        public List<BillItemEntryDto> ItemsArray { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal GstPercent { get; set; }
        public decimal GrandTotal { get; set; }

    }
}
