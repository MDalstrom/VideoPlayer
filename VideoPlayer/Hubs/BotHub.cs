using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VideoPlayer.Models;

namespace VideoPlayer.Hubs
{
    public class BotHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            BotPool.Instance.Received += async (sender, package) => await SendMessage(this, package);
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(object sender, Package package)
        {
            await Clients.All.SendAsync("ReceiveMessage", package.Json);
        }
    }
}
