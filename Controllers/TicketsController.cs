using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.Data;
using SupportTicketAPI.Models;
using SupportTicketAPI.Enums;
using SupportTicketAPI.Services;

namespace SupportTicketAPI.Controllers
{
    [Route("tickets")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly SupabaseService _supabaseService;

        public TicketsController(AppDbContext context,SupabaseService supabaseService)
        {
            _context = context;
            _supabaseService = supabaseService;
        }

        [HttpPost]
        [Authorize(Roles = "USER,MANAGER")]
        public IActionResult CreateTicket(Ticket ticket)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ticket.CreatedBy = userId;
            ticket.Status = TicketStatus.OPEN;

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            return StatusCode(201, ticket);
        }

        [HttpGet]
        public IActionResult GetTickets()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Ticket> tickets = _context.Tickets;

            if (role == "MANAGER")
            {
                return Ok(tickets.ToList());
            }
            else if (role == "SUPPORT")
            {
                return Ok(tickets.Where(t => t.AssignedTo == userId).ToList());
            }
            else
            {
                return Ok(tickets.Where(t => t.CreatedBy == userId).ToList());
            }
        }



        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not provided");

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            using var stream = file.OpenReadStream();

            var url = await _supabaseService.UploadStreamAsync(
                stream,
                fileName,
                file.ContentType
            );

            return Ok(new
            {
                message = "File uploaded",
                url = url
            });
        }


        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public IActionResult AssignTicket(int id, [FromBody] int assignedUserId)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket == null)
                return NotFound();

            var user = _context.Users.Include(u => u.Role)
                .FirstOrDefault(u => u.Id == assignedUserId);

            if (user == null)
                return BadRequest("User Not Found");

            if (user.Role.Name == "USER")
                return BadRequest("Cannot assign ticket to USER role");

            ticket.AssignedTo = assignedUserId;
            _context.SaveChanges();

            return Ok("ticket assigned successfully");
        }


        [HttpPatch("{id}/status")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public IActionResult UpdateStatus(int id, [FromBody] TicketStatus newStatus)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket == null)
                return NotFound();

            if (!IsValidTransition(ticket.Status, newStatus))
                return BadRequest("Invalid status transition");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var log = new TicketStatusLog
            {
                TicketId = ticket.Id,
                OldStatus = ticket.Status,
                NewStatus = newStatus,
                ChangedBy = userId
            };

            ticket.Status = newStatus;

            _context.TicketStatusLogs.Add(log);
            _context.SaveChanges();
            return Ok("Status update successfully");
    }
        [HttpDelete("{id}")]
        [Authorize(Roles = "MANAGER")]
        public IActionResult DeleteTicket(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if(ticket == null)
                return NotFound();

            _context.Tickets.Remove(ticket);
            _context.SaveChanges();
            return NoContent();
        }

        private bool IsValidTransition(TicketStatus oldstatus, TicketStatus newStatus)
        {
            return (oldstatus == TicketStatus.OPEN && newStatus == TicketStatus.IN_PROGRESS)
                || (oldstatus == TicketStatus.IN_PROGRESS && newStatus == TicketStatus.RESOLVED)
                || (oldstatus == TicketStatus.RESOLVED && newStatus == TicketStatus.CLOSED);
        }
    }
}