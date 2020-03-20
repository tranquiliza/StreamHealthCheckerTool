using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheckerHost
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HealthCheckerMain _healthCheckerMain;

        public Worker(ILogger<Worker> logger, HealthCheckerMain healthCheckerMain)
        {
            _logger = logger;
            _healthCheckerMain = healthCheckerMain;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _healthCheckerMain.StartAsync().ConfigureAwait(false);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _healthCheckerMain.StopAsync().ConfigureAwait(false);
        }
    }
}
