using System.ComponentModel.DataAnnotations;

namespace Billing_Backend.Dto
{
    public class BillItemEntryDto
    {
        public int ItemId { get; set; }
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
