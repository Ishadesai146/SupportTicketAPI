using SupportTicketAPI.Enums;

namespace SupportTicketAPI.Models
{
    public class TicketStatusLog
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public TicketStatus OldStatus { get; set; }
        public TicketStatus NewStatus { get; set; }
        public int ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;

    }
}
