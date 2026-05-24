using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing_Backend.Entity
{
    public class BillItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BillItemId { get; set; } // Kept as int for performance

        public Guid BillId { get; set; }   // Must be Guid to match Bill
        public int ItemId { get; set; }     // Must be int to match Item

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [NotMapped]
        public decimal Total => Quantity * Price;

        [ForeignKey("BillId")]
        public Bill? Bill { get; set; }

        [ForeignKey("ItemId")]
        public Item? Item { get; set; }
    }
}
