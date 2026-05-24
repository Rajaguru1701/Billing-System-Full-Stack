using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing_Backend.Entity
{
    public class Bill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid BillId { get; set; }

        public Guid CustomerId { get; set; } // Must be Guid to match Customer

        public DateTime BillDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GstAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();
    }
}
