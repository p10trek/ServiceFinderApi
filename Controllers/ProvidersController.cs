using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServiceFinderApi.Models;
using ServiceFinderApi.Models.RequestModels;
using ServiceFinderApi.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public async Task<ServiceResponse<List<GetProvidersView>>> Index()
        {
            return ServiceResponse<List<GetProvidersView>>.Ok(await _context.Providers.Select(row => new GetProvidersView
            {
                City = row.City,
                Description = row.Description,
                Email = row.Email,
                Id = row.Id,
                Lng = row.Lng,
                Lat = row.Lat,
                Name =row.Name,
                Number = row.Number,
                Phone = row.Phone,
                PostalCode = row.PostalCode,
                Street =  row.Street
                
            }).ToListAsync(), "User list successfully downloaded");
        }
        [HttpGet("/GetProfile")]
        [Authorize]
        public async Task<ServiceResponse<UserProfile>> Details()
        {
            string userLogin = User.Identity.Name;
            var user = await _context.Users.Where(row => row.Login == userLogin)
                        .Where(row => row.Login == User.Identity.Name).FirstOrDefaultAsync();

            if (user.IsProvider == true)
            {
                var provider = await _context.Providers.Select(r=>new UserProfile 
                { 
                    City = r.City,
                    Description = r.Description,
                    Email = r.Email,
                    Id = r.Id,
                    Lat = r.Lat,
                    Lng = r.Lng,
                    Logo = r.Logo,
                    Name = r.Name,
                    Number = r.Number,
                    Phone = r.Phone,
                    PostalCode = r.PostalCode,
                    Street = r.Street,
                    UserId = r.UserId
                }).FirstOrDefaultAsync(m => m.UserId == user.Id);
                if (provider == null)
                {
                    return ServiceResponse<UserProfile>.Error("Claims not match to any user");
                }

                return ServiceResponse<UserProfile>.Ok(provider, "Provider found");
            }
            else
            {
                return ServiceResponse<UserProfile>.Ok(new UserProfile
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Phone = user.Phone
                }, "User found");
            }

        }

        [HttpPut("/CreateProvider")]
        public async Task<ServiceResponse<bool>> Create(CreateProvider provider)
        {
            if (!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");
            if(_context.Users.Where(r=>r.Login == provider.Login).Any())
                return ServiceResponse<bool>.Error("User allready exists");
            Guid userId = Guid.NewGuid();
            User user = new User
            {
                Email = provider.Email,
                Id = userId,
                IsProvider = provider.IsProvider,
                Login = provider.Login,
                Name = provider.Name,
                Password = provider.Password,
                Phone = provider.Phone,
            };
            _context.Add(user);

            if (provider.IsProvider)
            {
                HttpClient client = new HttpClient();

                var result = await client.GetStringAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={provider.City}+{provider.Street}+{provider.Number}&key=AIzaSyB2ji39iC4YHI6oOY1ryEwufydeosQH0oE");
                
                var location = JsonConvert.DeserializeObject<GetCoordinatessModel>(result);
                decimal lat = location.results[0].geometry.location.lat;
                decimal lng = location.results[0].geometry.location.lng;

                Provider providerToAdd = new Provider
                {
                    Id = Guid.NewGuid(),
                    //Login = provider.Login,
                    //Password = BCrypt.Net.BCrypt.HashPassword(provider.Password),
                    Email = provider.Email,
                    Name = provider.Name,
                    Phone = provider.Phone,
                    City = provider.City,
                    PostalCode = provider.PostalCode,
                    Description = provider.Description,
                    Logo = provider.Logo,
                    Number = provider.Number,
                    Street = provider.Street,
                    UserId = userId,
                    Lat = lat,
                    Lng = lng
                };
                _context.Add(providerToAdd);
            }
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "User was added");
        }
        [HttpPost("/EditProvider")]
        [Authorize(Roles = "Manager")]
        public async Task<ServiceResponse<bool>> Edit(EditProvider editedProvider)
        {
            var provider = await(from u in _context.Users.Where(row => row.IsProvider == true)
                                  .Where(row => row.Login == User.Identity.Name)
                                join prov in _context.Providers on u.Id equals prov.UserId
                                select prov).Select(r => r).FirstOrDefaultAsync();

            if (provider == null)
            {
                return ServiceResponse<bool>.Error("User not Found");
            }
            var user = await _context.Users.Where(r => r.Id == provider.UserId).FirstOrDefaultAsync();

            if (!String.IsNullOrEmpty(editedProvider.Name))
                provider.Name = editedProvider.Name;
            if (!String.IsNullOrEmpty(editedProvider.NewPassword))
                user.Password = BCrypt.Net.BCrypt.HashPassword(editedProvider.NewPassword);
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
        //[HttpPost("/LoginProvider")]
        //public async Task<ServiceResponse<bool>> Login(LoginProvider provider)
        //{
        //    if (!ModelState.IsValid)
        //        return ServiceResponse<bool>.Error("User adding error");
        //    var providerToLogin = await _context.Providers.Where(row => row.Login == provider.Login).FirstOrDefaultAsync();

        //    if (providerToLogin == null)
        //        return ServiceResponse<bool>.Error("User not found");

        //    if (providerToLogin.Login == provider.Login)
        //    {
        //        if (BCrypt.Net.BCrypt.Verify(provider.Password, providerToLogin.Password))
        //            return ServiceResponse<bool>.Ok(true, "User loged in");
        //    }
        //    return ServiceResponse<bool>.Error("Incorrect login or password");
        //}
        //[HttpDelete("/DeleteProvider")]
        //public async Task<ServiceResponse<bool>> Delete(string login)
        //{
        //    var provider = await _context.Providers.Where(row => row.Login == login).FirstOrDefaultAsync();
        //    if (provider == null)
        //    {
        //        return ServiceResponse<bool>.Error("User not found");
        //    }

        //    _context.Remove(provider);
        //    await _context.SaveChangesAsync();

        //    return ServiceResponse<bool>.Ok(true, "User succesfully deleted");
        //}
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
