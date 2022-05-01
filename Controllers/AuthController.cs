using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceFinderApi.Models;
using ServiceFinderApi.Models.RequestModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFinderApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : MyControllerBase
    {
        private readonly ServiceFinderDBContext _context;
        public AuthController(ServiceFinderDBContext context)
        {
            _context = context;
        }
        [HttpPost, Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel user)
        {

            if (user == null)
            {
                return BadRequest("Invalid client request");
            }
            var dbUser = await _context.Users
                .Where(row => row.Login == user.UserName)
                .Where(row => row.Password == user.Password)
                .FirstOrDefaultAsync();

            if (dbUser == null)
            {
                return Unauthorized();
            }
            else
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, dbUser.IsProvider?"Manager":"User")
                };
                var tokeOptions = new JwtSecurityToken(
                    issuer: "https://localhost:44309",
                    audience: "https://localhost:44309",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: signinCredentials
                );
                Guid provId = Guid.Empty;
                if (dbUser.IsProvider)
                    provId = await _context.Providers
                        .Where(r => r.UserId == dbUser.Id)
                        .Select(r=>r.Id).FirstOrDefaultAsync();

                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                return Ok(new { Token = tokenString, login = user.UserName, isProvider = dbUser.IsProvider, providerId = provId });
            }
        }
    }
}
