using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"ReceiveMessage: {user} {message}");
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}