using System;
using System.Collections.Generic;

namespace StreamHealthChecker.Core.Model
{
    public class User
    {
        public TimeSpan NoObsCheckInterval { get; private set; } = TimeSpan.FromMinutes(5);
        public TimeSpan NotLiveCheckInternval { get; private set; } = TimeSpan.FromMinutes(5);
        public TimeSpan LiveCheckInterval { get; private set; } = TimeSpan.FromSeconds(10);
        public TimeSpan LiveCheckJustSentErrorInterval { get; private set; } = TimeSpan.FromSeconds(10);

        private readonly List<string> _poorConnectionMessages = new List<string>();
        public IReadOnlyList<string> PoorConnectionMessages => _poorConnectionMessages;

        private readonly List<string> _noConnectionMessages = new List<string>();
        public IReadOnlyList<string> NoConnectionMessages => _noConnectionMessages;

        public string HealthApiUrl { get; private set; }
        public int MinimumBitrate { get; private set; }
        public string Username { get; private set; }

        [Obsolete("For serialization Only", true)]
        public User() { }

        public User(string username, int minimumBitrate, string healthApiUrl)
        {
            Username = username;
            MinimumBitrate = minimumBitrate;
            HealthApiUrl = healthApiUrl;
        }

        public void AddPoorConnectionMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty", nameof(message));

            _poorConnectionMessages.Add(message);
        }
        public void AddNoConnectionMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty", nameof(message));

            _noConnectionMessages.Add(message);
        }
    }
}
