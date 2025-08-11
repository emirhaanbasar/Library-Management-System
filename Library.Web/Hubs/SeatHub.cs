using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Library.Web.Hubs
{
    public class SeatHub : Hub
    {
        public async Task NotifySeatChanged()
        {
            await Clients.All.SendAsync("SeatChanged");
        }
    }
} 