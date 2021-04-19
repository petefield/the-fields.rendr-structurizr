using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace web.Hubs
{
    public class ChatHub : Hub
    {
        public async Task Refresh()
        {
            await Clients.All.SendAsync("Refresh");
        }
    }
}