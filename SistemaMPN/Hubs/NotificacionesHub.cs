using Microsoft.AspNetCore.SignalR;
using SistemaMPN.Shared.DTO;

namespace SistemaMPN.Hubs
{
    public class NotificacionesHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
