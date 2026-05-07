namespace HRConnect.Api.Hubs
{
    using Microsoft.AspNetCore.SignalR;

    public class LeaveHub : Hub
    {
        public async Task JoinEmployeeGroup(string employeeId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                employeeId
            );
        }

        public async Task LeaveEmployeeGroup(string employeeId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                employeeId
            );
        }
    }
}