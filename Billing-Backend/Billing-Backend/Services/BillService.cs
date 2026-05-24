using Billing_Backend.Data;
using Billing_Backend.Dto;
using Billing_Backend.Entity;

namespace Billing_Backend.Services
{
    public class BillService
    {
        private readonly AppDbContext _context;

        public BillService(AppDbContext context)
        {
            _context = context;
        }                       
        public async Task<Bill> SaveBillAsync(BillDto dto)
        {
            // 1. Calculate the explicit GST Absolute Amount from parameters
            decimal computedGstAmount = dto.SubTotal * (dto.GstPercent / 100);

            // 2. Map Parent Entry parameters to Bill Entity
            var bill = new Bill
            {
                CustomerId = dto.CustomerId,
                BillDate = DateTime.UtcNow,
                GstAmount = Math.Round(computedGstAmount, 2),
                TotalAmount = dto.GrandTotal
            };

            // 3. Loop through dynamic rows array and map child entities
            foreach (var itemDto in dto.ItemsArray)
            {
                var billItem = new BillItem
                {
                    ItemId = itemDto.ItemId,
                    Quantity = itemDto.Quantity,
                    Price = itemDto.UnitPrice
                    // Note: 'Total' property is marked [NotMapped], computing on the fly!
                };

                bill.BillItems.Add(billItem);
            }

            // 4. Save entire object graph atomically using EF Core tracking
            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            return bill;
        }
    }
}
