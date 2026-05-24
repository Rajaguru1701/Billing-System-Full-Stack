using Billing_Backend.Data;
using Billing_Backend.Dto;
using Billing_Backend.Entity;
using Microsoft.EntityFrameworkCore;

namespace Billing_Backend.Services
{
    public class ItemService
    {
        private readonly AppDbContext _context;

        public ItemService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<object> CreateNewItemAsync(ItemDto itemDto)
        {
            Item item = new Item
            {
                ItemName = itemDto.ItemName,
                UnitPrice = itemDto.UnitPrice,
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return new
            {
                Message = "Item Added successfully",
            };
        }

        public async Task<List<Item>> GetAllItems()
        {
            return await _context.Items.ToListAsync();
        }
    }
}
