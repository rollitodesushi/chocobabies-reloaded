using Microsoft.AspNetCore.SignalR;

namespace ChocobabiesReloaded.Hubs
{
    public class RifaHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
