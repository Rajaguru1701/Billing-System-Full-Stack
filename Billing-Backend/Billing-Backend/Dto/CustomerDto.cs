namespace Billing_Backend.Dto
{
    public class CustomerDto
    {
        public string Name { get; set; }
        public string PhoneNo { get; set; }
        public string? Address { get; set; } = string.Empty;
    }
}
