using System;
using System.Collections.Generic;
using System.Text;

namespace StreamHealthChecker.TwitchIntegration
{
    public class TwitchClientSettings
    {
        public string TwitchUsername { get; }
        public string TwitchBotOAuth { get; }

        public TwitchClientSettings(string twitchUsername, string twitchBotOAuth)
        {
            TwitchUsername = twitchUsername;
            TwitchBotOAuth = twitchBotOAuth;
        }
    }
}
