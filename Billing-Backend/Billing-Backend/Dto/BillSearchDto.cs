namespace Billing_Backend.Dto
{
    public class BillSearchDto
    {
        public string? CustomerName { get; set; }
        public string? PhoneNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
