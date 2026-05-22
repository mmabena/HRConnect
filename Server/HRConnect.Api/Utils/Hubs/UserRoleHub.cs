namespace HRConnect.Api.Utils.Hubs
{
  using Microsoft.AspNetCore.SignalR;

  public class UserRoleHub : Hub
  {
    public async Task SendUpdate(string user, string payload)
    {
      await Clients.All.SendAsync("RecieveMessage",user,payload);
    }
  }
}