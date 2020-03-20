using System;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;

namespace StreamHealthChecker.TwitchIntegration
{
    public class TwitchChatClient : ITwitchIntegration
    {
        private readonly ITwitchClient _twitchClient;

        public TwitchChatClient(TwitchClientSettings twitchClientSettings)
        {
            var credentials = new ConnectionCredentials(twitchClientSettings.TwitchUsername, twitchClientSettings.TwitchBotOAuth);
            _twitchClient = new TwitchClient();

            _twitchClient.Initialize(credentials);
        }

        private TaskCompletionSource<bool> _connectionCompletionTask = new TaskCompletionSource<bool>();

        public event EventHandler OnConnected;

        public async Task Connect()
        {
            _twitchClient.OnConnected += TwitchClient_OnConnected;
            _twitchClient.Connect();

            await _connectionCompletionTask.Task.ConfigureAwait(false);
        }

        public async Task SendMessage(string channel, string message)
        {
            await Task.Run(() =>
            {
                if (_twitchClient.IsConnected)
                {
                    _twitchClient.SendMessage(channel, message);
                }
            }).ConfigureAwait(false);
        }

        private void TwitchClient_OnConnected(object sender, OnConnectedArgs e)
        {
            _twitchClient.OnConnected -= TwitchClient_OnConnected;

            OnConnected?.Invoke(this, null);

            _connectionCompletionTask.SetResult(true);
            _connectionCompletionTask = new TaskCompletionSource<bool>();
        }

        private TaskCompletionSource<bool> _joinChannelCompletionTask = new TaskCompletionSource<bool>();

        public async Task JoinChannel(string channelName)
        {
            if (_twitchClient.JoinedChannels.Any(x => string.Equals(channelName, x.Channel, StringComparison.OrdinalIgnoreCase)))
                return;

            _twitchClient.JoinChannel(channelName);
            _twitchClient.OnJoinedChannel += TwitchClient_OnJoinedChannel;

            await _joinChannelCompletionTask.Task.ConfigureAwait(false);
        }

        private void TwitchClient_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            _twitchClient.OnJoinedChannel -= TwitchClient_OnJoinedChannel;

            _joinChannelCompletionTask.SetResult(true);
            _joinChannelCompletionTask = new TaskCompletionSource<bool>();
        }

        public void Dispose()
        {
            _twitchClient.Disconnect();
        }
    }
}
