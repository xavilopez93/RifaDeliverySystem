using Microsoft.AspNetCore.SignalR;
namespace RifaDeliverySystem.Web.Hubs
{
    public class DashboardHub : Hub
    {
        public async Task SendUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveUpdate", message);
        }
    }
}
