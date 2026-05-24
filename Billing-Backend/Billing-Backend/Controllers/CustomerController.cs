using Billing_Backend.Dto;
using Billing_Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Billing_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost("save-customer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto customer)
        {
            // Safely validates that incoming customer payload body isn't null
            if (customer == null)
            {
                return BadRequest("Customer data cannot be empty.");
            }

            // Awaits the task to ensure proper save state completion in PostgreSQL
            var res = await _customerService.CreateNewCustomerAsync(customer);

            return Ok(res);
        }

        [HttpGet("get-customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var res = await _customerService.GetAllCustomers();

            return Ok(res);
        }
    }
}