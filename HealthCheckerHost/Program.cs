using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StreamHealthChecker.Core;
using StreamHealthChecker.Repository.Sql;
using StreamHealthChecker.TwitchIntegration;

namespace HealthCheckerHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;
                    var botName = config.GetValue<string>("ChatbotClient:BotUserName");
                    var botOAuthToken = config.GetValue<string>("ChatbotClient:BotOAuthToken");
                    var clientSettings = new TwitchClientSettings(botName, botOAuthToken);

                    services.AddSingleton<UserJobController, UserJobController>();
                    services.AddSingleton<IUserRepository>(new UserRepository(config.GetValue<string>("StreamHealthApiAddress")));
                    services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
                    services.AddSingleton<IStreamHealthLogger, StreamHealthFileLogger>();
                    services.AddSingleton(clientSettings);
                    services.AddSingleton<ITwitchIntegration, TwitchChatClient>();
                    services.AddSingleton<HealthCheckerMain, HealthCheckerMain>();
                    services.AddHostedService<Worker>();
                });
    }
}
