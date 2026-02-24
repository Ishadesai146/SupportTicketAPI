using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportTicketAPI.Data;
using SupportTicketAPI.DTOs;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Controllers
{
    [Route("users")]
    [ApiController]
    [Authorize(Roles = "MANAGER")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateUser(CreateUserDTO dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email alredy exists");

            var role = _context.Roles.Find(dto.RoleId);
            if (role == null)
                return BadRequest("Invalid Role");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = hashedPassword,
                RoleId = dto.RoleId
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return StatusCode(201, "User created successfully");
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Role = u.Role.Name

                })
                .ToList();

            return Ok(users);
        }
    }
}
