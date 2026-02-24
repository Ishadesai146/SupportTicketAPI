using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SupportTicketAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SupportTicketAPI.Data;

namespace SupportTicketAPI.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(x => x.Email == dto.Email);

            if (user == null)
                return Unauthorized();

            if(!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );
            //Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Isha@123"));
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token)});
        }
    }
}
