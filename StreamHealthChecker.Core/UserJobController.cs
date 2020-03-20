using Microsoft.Extensions.Logging;
using StreamHealthChecker.Core.Application;
using StreamHealthChecker.TwitchIntegration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamHealthChecker.Core
{
    public class UserJobController : IDisposable
    {
        private readonly ITwitchIntegration _twitchIntegration;
        private readonly IUserRepository _userRepository;
        private readonly IStreamHealthLogger _streamHealthLogger;

        private readonly List<UserJob> _userJobs = new List<UserJob>();
        private readonly ILogger<UserJobController> _logger;

        public UserJobController(ITwitchIntegration twitchIntegration, IUserRepository userRepository, IStreamHealthLogger streamHealthLogger, ILogger<UserJobController> logger)
        {
            _twitchIntegration = twitchIntegration;
            _userRepository = userRepository;
            _streamHealthLogger = streamHealthLogger;
            _logger = logger;
        }

        public async Task StartMonitoring()
        {
            var users = await _userRepository.GetUsers().ConfigureAwait(false);
            foreach (var user in users)
            {
                var userJob = new UserJob(user, _twitchIntegration, _streamHealthLogger);
                userJob.Run();
                _userJobs.Add(userJob);
                _logger.LogInformation($"Started job for {user.Username}");
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _userJobs.Count; i++)
                _userJobs[i].Dispose();
        }
    }
}
