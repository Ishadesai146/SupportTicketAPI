using Microsoft.AspNetCore.SignalR;

namespace SupportTicketAPI.Services
{
    public class NotificationHub : Hub
    {
        // 1. BROADCAST: Everyone connected to this Hub sees this
        public async Task SendToEveryone(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", "System", message);
        }

        // 2. GROUP: Only users in a specific "Room" see this
        public async Task JoinGroup(string groupName)
        {
            // Adds the current connection to a group (e.g., "AdminOnly" or "ChatRoom1")
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} joined {groupName}");
        }

        public async Task SendToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "Group Alert", message);
        }

    }
}
