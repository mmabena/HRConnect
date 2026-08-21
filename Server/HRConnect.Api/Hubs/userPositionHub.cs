namespace HRConnect.Api.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;

    [Authorize(Roles = "SuperUser")]
    public class UserPositionHub : Hub
    {
        public async Task SendPositionUpdate(string employeeId, string newPosition)
        {
            await Clients.Others.SendAsync(
                "ReceivePositionUpdate",
                employeeId,
                newPosition
            );
        }
    }
}