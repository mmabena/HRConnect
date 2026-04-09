namespace HRConnect.Api.Hubs
{
     using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
    public class UserPositionHub : Hub
    {
        
        // This method will be called by the client when an employee's position changes
        public async Task SendPositionUpdate(int employeeId, string newPosition)
        {
            // Broadcast to all connected clients except the sender
            await Clients.Others.SendAsync("ReceivePositionUpdate", employeeId, newPosition);
        }
    }
}