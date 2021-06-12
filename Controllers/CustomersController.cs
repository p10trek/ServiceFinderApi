using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServiceFinderApi.Models;
using ServiceFinderApi.Models.RequestModels;

namespace ServiceFinderApi.Controllers
{
    public class CustomersController : MyControllerBase
    {
        private readonly ServiceFinderDBContext _context;

        public CustomersController(ServiceFinderDBContext context)
        {
            _context = context;
        }

        [HttpGet("/GetUsers")]
        public async Task<ServiceResponse<List<Customer>>> Index()
        {
            return ServiceResponse<List<Customer>>.Ok(await _context.Customers.ToListAsync(), "User list successfully downloaded");
        }

        [HttpPost("/GetUser")]
        public async Task<ServiceResponse<Customer>> Details(Guid? id)
        {
            if (id == null)
            {
                return ServiceResponse<Customer>.Error("Id parameter is null");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return ServiceResponse<Customer>.Error("Id not match to any customer");
            }

            return ServiceResponse<Customer>.Ok(customer, "Cusotmer found");
        }

        [HttpPut("/CreateCustomer")]
        public async Task<ServiceResponse<bool>> Create([Bind("Id,Login,Password,Name,Phone,Email")] CreateCustomer customer)
        {
            if(!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");

            Customer customerToAdd = new Customer
            {
                Id = Guid.NewGuid(),
                Login = customer.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(customer.Password),
                Email = customer.Email,
                Name = customer.Name,
                Phone = customer.Phone
            };
        
            _context.Add(customerToAdd);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "User was added");
        }

        [HttpPost("/EditCustomer")]
        public async Task<ServiceResponse<bool>> Edit(EditCustomer editedCustomer)
        {
            var customer = await _context.Customers
                                        .Where(row => row.Login == editedCustomer.Login)
                                        .FirstOrDefaultAsync();
            if (customer == null)
            {
                return ServiceResponse<bool>.Error("User not Found"); ;
            }
            if (!String.IsNullOrEmpty(editedCustomer.Name))
                customer.Name = editedCustomer.Name;
            if (!String.IsNullOrEmpty(editedCustomer.NewPassword))
                customer.Password = BCrypt.Net.BCrypt.HashPassword(editedCustomer.NewPassword);
            if (!String.IsNullOrEmpty(editedCustomer.Phone))
                customer.Phone = editedCustomer.Phone;

            _context.Update(customer);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true,"User edited corectly");
        }

        [HttpPost("/LoginCustomer")]
        public async Task<ServiceResponse<bool>> Login(LoginCustomer customer)
        {
            if(!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");
            var customerToLogin = await _context.Customers.Where(row => row.Login == customer.Login).FirstOrDefaultAsync();
            
            if(customerToLogin==null)
                return ServiceResponse<bool>.Error("User not found");

            if(customerToLogin.Login == customer.Login)
            {
                if(BCrypt.Net.BCrypt.Verify(customer.Password,customerToLogin.Password))
                    return ServiceResponse<bool>.Ok(true, "User loged in");
            }
            return ServiceResponse<bool>.Error("Incorrect login or password");
        }
        [HttpDelete("/DeleteCustomer")]
        public async Task<ServiceResponse<bool>>  Delete(string login)
        {
            var customer = await _context.Customers.Where(row => row.Login == login).FirstOrDefaultAsync();
            if (customer == null)
            {
                return ServiceResponse<bool>.Error("User not found");
            }

            _context.Remove(customer);
            await _context.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "User succesfully deleted");
        }

        [HttpGet("/IfCustomerExist")]
        public async Task<ServiceResponse<bool>> CustomerExists(Guid id)
        {
            bool isExist = await _context.Customers.AnyAsync(e => e.Id == id);
            if(isExist)
                return ServiceResponse<bool>.Ok(true, "User exist");
            return ServiceResponse<bool>.Error("User don't exist");
        }
    }
}
