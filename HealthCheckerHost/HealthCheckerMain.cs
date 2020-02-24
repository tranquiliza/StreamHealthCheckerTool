using Microsoft.Extensions.Logging;
using StreamHealthChecker.Core;
using StreamHealthChecker.TwitchIntegration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheckerHost
{
    public class HealthCheckerMain
    {
        private readonly ITwitchIntegration _twitchIntegration;
        private readonly UserJobController _backpackVideoFeedHealthChecker;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<HealthCheckerMain> _logger;

        public HealthCheckerMain(ITwitchIntegration twitchIntegration, IUserRepository userRepository, UserJobController backpackVideoFeedHealthChecker, ILogger<HealthCheckerMain> logger)
        {
            _twitchIntegration = twitchIntegration ?? throw new ArgumentNullException(nameof(twitchIntegration));

            _twitchIntegration.OnConnected += async (sender, eventArgs) => await TwitchIntegration_OnConnected().ConfigureAwait(false);
            _backpackVideoFeedHealthChecker = backpackVideoFeedHealthChecker;
            _userRepository = userRepository;
            _logger = logger;
        }

        private async Task TwitchIntegration_OnConnected()
        {
            var users = await _userRepository.GetUsers().ConfigureAwait(false);
            foreach (var user in users)
            {
                _logger.LogInformation($"Joining {user.Username}");
                await _twitchIntegration.JoinChannel(user.Username.ToLower()).ConfigureAwait(false);
                _logger.LogInformation($"Joined {user.Username}");
            }
        }

        public async Task StartAsync()
        {
            await _twitchIntegration.Connect().ConfigureAwait(false);
            await _backpackVideoFeedHealthChecker.StartMonitoring().ConfigureAwait(false);
        }

        public Task StopAsync()
        {
            _backpackVideoFeedHealthChecker.Dispose();
            _twitchIntegration.Dispose();

            return Task.CompletedTask;
        }
    }
}
