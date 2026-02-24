using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportTicketAPI.Data;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Controllers
{
    [Route("")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("tickets/{id}/comments")]
        public IActionResult AddComment(int id, [FromBody] TicketComment model)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket == null)
                return NotFound("Ticket Not Found");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (!HasTicketAccess(ticket, userId, role))
                return Forbid();

            model.TicketId = id;
            model.UserId = userId;
            _context.TicketComments.Add(model);
            _context.SaveChanges();
            return StatusCode(201, model);
        }

        [HttpGet("tickets/{id}/comments")]
        public IActionResult GetComments(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket == null)
                return NotFound("Ticket Not Found");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (!HasTicketAccess(ticket, userId, role))
                return Forbid();

            var comments = _context.TicketComments
                .Where(c => c.TicketId == id)
                .Include(c => c.User)
                    .ThenInclude(u => u.Role)
                .Select(c => new
                {
                    id = c.Id,
                    comment = c.Comment,
                    user = new
                    {
                        id = c.User.Id,
                        name = c.User.Name,
                        email = c.User.Email,
                        role = new
                        {
                            id = c.User.Role.Id,
                            name = c.User.Role.Name
                        },
                        created_at = c.User.CreatedAt
                    },
                    created_at = c.CreatedAt
                })
                        .ToList();

            return Ok(comments);
        }

        [HttpPatch("comments/{id}")]
        public IActionResult EditComment(int id, [FromBody] string updatedComment)
        {
            var comment = _context.TicketComments.Find(id);
            if(comment != null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "MANAGER" && comment.UserId != userId)
                return Forbid();

            comment.Comment = updatedComment;
            _context.SaveChanges();

            return Ok("comment update successfully");
        }

        [HttpDelete("comments/{id}")]
        public IActionResult DeleteComment(int id)
        {
            var comment = _context.TicketComments.Find(id);
            if(comment == null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            if(role != "MANAGER" && comment.UserId != userId)
                return Forbid();

            _context.TicketComments.Remove(comment);
            _context.SaveChanges();

            return NoContent();
        }

        private bool HasTicketAccess(Ticket ticket , int userId, string role)
        {
            if (role == "MANAGER")
                return true;

            if (role == "SUPPORT" && ticket.AssignedTo == userId)
                return true;

            if(role == "USER" && ticket.CreatedBy == userId)
                return true;

            return false;
        }

    }
}
