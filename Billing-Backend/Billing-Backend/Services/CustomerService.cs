using Billing_Backend.Data;
using Billing_Backend.Dto;
using Billing_Backend.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks; // Explicit safety import

namespace Billing_Backend.Services
{
    public class CustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> CreateNewCustomerAsync(CustomerDto customerDto)
        {
            Customer customer = new Customer
            {
                Name = customerDto.Name,
                PhoneNo = customerDto.PhoneNo,
                Address = customerDto.Address
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return new
            {
                Message = "Customer created successfully"
            };
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            return await _context.Customers.ToListAsync();
        }
    }
}