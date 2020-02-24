using System;
using System.Threading.Tasks;

namespace StreamHealthChecker.TwitchIntegration
{
    public interface ITwitchIntegration : IDisposable
    {
        event EventHandler OnConnected;
        Task Connect();
        Task SendMessage(string channel, string message);
        Task JoinChannel(string channelName);
    }
}