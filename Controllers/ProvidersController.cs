using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinderApi.Models;
using ServiceFinderApi.Models.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Controllers
{
    public class ProvidersController : MyControllerBase
    {
        private readonly ServiceFinderDBContext _context;
        public ProvidersController(ServiceFinderDBContext context)
        {
            _context = context;
        }
        [HttpGet("/GetProviders")]
        public async Task<ServiceResponse<List<Provider>>> Index()
        {
            return ServiceResponse<List<Provider>>.Ok(await _context.Providers.ToListAsync(), "User list successfully downloaded");
        }
        [HttpPost("/GetProvider")]
        public async Task<ServiceResponse<Provider>> Details(Guid? id)
        {
            if (id == null)
            {
                return ServiceResponse<Provider>.Error("Id parameter is null");
            }

            var provider = await _context.Providers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (provider == null)
            {
                return ServiceResponse<Provider>.Error("Id not match to any provider");
            }

            return ServiceResponse<Provider>.Ok(provider, "Provider found");
        }

        [HttpPut("/CreateProvider")]
        public async Task<ServiceResponse<bool>> Create(CreateProvider provider)
        {
            if (!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");

            Provider providerToAdd = new Provider
            {
                Id = Guid.NewGuid(),
                Login = provider.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(provider.Password),
                Email = provider.Email,
                Name = provider.Name,
                Phone = provider.Phone,
                City = provider.City,
                PostalCode = provider.PostalCode,
                Description = provider.Description,
                Logo = provider.Logo,
                Number = provider.Number,
                Street = provider.Street
            };

            _context.Add(providerToAdd);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "User was added");
        }
        [HttpPost("/EditProvider")]
        public async Task<ServiceResponse<bool>> Edit(EditProvider editedProvider)
        {
            var provider = await _context.Providers
                                        .Where(row => row.Login == editedProvider.Login)
                                        .FirstOrDefaultAsync();
            if (provider == null)
            {
                return ServiceResponse<bool>.Error("User not Found");
            }
            if (!String.IsNullOrEmpty(editedProvider.Name))
                provider.Name = editedProvider.Name;
            if (!String.IsNullOrEmpty(editedProvider.NewPassword))
                provider.Password = BCrypt.Net.BCrypt.HashPassword(editedProvider.NewPassword);
            if (!String.IsNullOrEmpty(editedProvider.Phone))
                provider.Phone = editedProvider.Phone;
            if (!String.IsNullOrEmpty(editedProvider.Logo))
                provider.Logo = editedProvider.Logo;
            if (!String.IsNullOrEmpty(editedProvider.Number))
                provider.Number = editedProvider.Number;
            if (!String.IsNullOrEmpty(editedProvider.PostalCode))
                provider.PostalCode = editedProvider.PostalCode;
            if (!String.IsNullOrEmpty(editedProvider.Street))
                provider.Street = editedProvider.Street;
            if (!String.IsNullOrEmpty(editedProvider.Description))
                provider.Description = editedProvider.Description;
            if (!String.IsNullOrEmpty(editedProvider.Email))
                provider.Email = editedProvider.Email;
            if (!String.IsNullOrEmpty(editedProvider.City))
                provider.City = editedProvider.City;

                _context.Update(provider);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "User edited corectly");
        }
        [HttpPost("/LoginProvider")]
        public async Task<ServiceResponse<bool>> Login(LoginProvider provider)
        {
            if (!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");
            var providerToLogin = await _context.Providers.Where(row => row.Login == provider.Login).FirstOrDefaultAsync();

            if (providerToLogin == null)
                return ServiceResponse<bool>.Error("User not found");

            if (providerToLogin.Login == provider.Login)
            {
                if (BCrypt.Net.BCrypt.Verify(provider.Password, providerToLogin.Password))
                    return ServiceResponse<bool>.Ok(true, "User loged in");
            }
            return ServiceResponse<bool>.Error("Incorrect login or password");
        }
        [HttpDelete("/DeleteProvider")]
        public async Task<ServiceResponse<bool>> Delete(string login)
        {
            var provider = await _context.Providers.Where(row => row.Login == login).FirstOrDefaultAsync();
            if (provider == null)
            {
                return ServiceResponse<bool>.Error("User not found");
            }

            _context.Remove(provider);
            await _context.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "User succesfully deleted");
        }
        [HttpGet("/IfProviderExist")]
        public async Task<ServiceResponse<bool>> ProviderExists(Guid id)
        {
            bool isExist = await _context.Providers.AnyAsync(e => e.Id == id);
            if (isExist)
                return ServiceResponse<bool>.Ok(true, "User exist");
            return ServiceResponse<bool>.Error("User don't exist");
        }
    }
}
