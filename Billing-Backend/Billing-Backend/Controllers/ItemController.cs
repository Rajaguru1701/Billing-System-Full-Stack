using Billing_Backend.Dto;
using Billing_Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
         private readonly ItemService _itemService;
    
         public ItemController(ItemService itemService)
         {
            _itemService = itemService;
         }

        [HttpPost("save-item")]
        public async Task<IActionResult> CreateItem([FromBody] ItemDto itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest("Item data cannot be empty.");
            }
            var res = await _itemService.CreateNewItemAsync(itemDto);

            return Ok(res);
        }

        [HttpGet("get-items")]
        public async Task<IActionResult> GetItems()
        {
            var res = await _itemService.GetAllItems();

            return Ok(res);
        }


    }
}
