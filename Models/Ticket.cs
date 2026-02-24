using System.ComponentModel.DataAnnotations;
using SupportTicketAPI.Enums;

namespace SupportTicketAPI.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        [Required]
        [MinLength(5)]
        public string Title { get; set; }

        [Required]
        [MinLength(10)]
        public string Description { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.OPEN;
        public Priority Priority { get; set; } = Priority.MEDIUM;
        public int CreatedBy { get; set; }
        public int? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
