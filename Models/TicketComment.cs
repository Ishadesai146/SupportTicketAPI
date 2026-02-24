using System.ComponentModel.DataAnnotations;

namespace SupportTicketAPI.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [MinLength(2)]
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
