using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServiceFinderApi.Models;
using ServiceFinderApi.Models.RequestModels;

namespace ServiceFinderApi.Controllers
{
    public class UserController : MyControllerBase
    {
        private readonly ServiceFinderDBContext _context;

        public UserController(ServiceFinderDBContext context)
        {
            _context = context;
        }


        [HttpGet("/GetUsers"), Authorize]
        public async Task<ServiceResponse<List<User>>> Index()
        {
            return ServiceResponse<List<User>>.Ok(await _context.Users.ToListAsync(), "User list successfully downloaded");
        }

        [HttpPost("/GetUser")]
        public async Task<ServiceResponse<User>> Details(Guid? id)
        {
            if (id == null)
            {
                return ServiceResponse<User>.Error("Id parameter is null");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return ServiceResponse<User>.Error("Id not match to any user");
            }

            return ServiceResponse<User>.Ok(user, "Cusotmer found");
        }

        [HttpPut("/CreateUser")]
        [Authorize(Roles = "Operator")]
        public async Task<ServiceResponse<bool>> Create([Bind("Id,Login,Password,Name,Phone,Email")] CreateUser user)
        {
            if(!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");

            User userToAdd = new User
            {
                Id = Guid.NewGuid(),
                Login = user.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Email = user.Email,
                Name = user.Name,
                Phone = user.Phone
            };
        
            _context.Add(userToAdd);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "User was added");
        }

        [HttpPost("/EditUser")]
        [Authorize]
        public async Task<ServiceResponse<bool>> Edit(EditUser editedUser)
        {

            var user = await _context.Users
                                        .Where(row => row.Login == User.Identity.Name)
                                        .FirstOrDefaultAsync();
            if (user == null)
            {
                return ServiceResponse<bool>.Error("User not Found"); ;
            }
            if (!String.IsNullOrEmpty(editedUser.Name))
                user.Name = editedUser.Name;
            if (!String.IsNullOrEmpty(editedUser.Email))
                user.Email = editedUser.Email;
            if (!String.IsNullOrEmpty(editedUser.Phone))
                user.Phone = editedUser.Phone;

            _context.Update(user);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true,"User edited corectly");
        }

        [HttpPost("/LoginUser")]
        public async Task<ServiceResponse<bool>> Login(LoginUser user)
        {
            if(!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");
            var userToLogin = await _context.Users.Where(row => row.Login == user.Login).FirstOrDefaultAsync();
            
            if(userToLogin==null)
                return ServiceResponse<bool>.Error("User not found");

            if(userToLogin.Login == user.Login)
            {
                if(BCrypt.Net.BCrypt.Verify(user.Password,userToLogin.Password))
                    return ServiceResponse<bool>.Ok(true, "User loged in");
            }
            return ServiceResponse<bool>.Error("Incorrect login or password");
        }
        [HttpDelete("/DeleteUser")]
        public async Task<ServiceResponse<bool>>  Delete(string login)
        {
            var user = await _context.Users.Where(row => row.Login == login).FirstOrDefaultAsync();
            if (user == null)
            {
                return ServiceResponse<bool>.Error("User not found");
            }

            _context.Remove(user);
            await _context.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "User succesfully deleted");
        }

        [HttpGet("/IfUserExist")]
        public async Task<ServiceResponse<bool>> UserExists(Guid id)
        {
            bool isExist = await _context.Users.AnyAsync(e => e.Id == id);
            if(isExist)
                return ServiceResponse<bool>.Ok(true, "User exist");
            return ServiceResponse<bool>.Error("User don't exist");
        }
    }
}
