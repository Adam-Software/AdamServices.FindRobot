using FindRobot.Core;
using FindRobot.Interface;
using FindRobot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;

namespace FindRobot
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)

                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.Sources.Clear();
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    })

                    .ConfigureServices((context, services) =>
                    {
                        AppSettingsOptionsService options = new();
                        context.Configuration.GetRequiredSection("AppSettingsOptions").Bind(options);
                        services.AddSingleton<IAppSettingsOptionsService>(options);

                        services.AddLogging(loggingBuilder =>
                        {
                            var logger = new LoggerConfiguration()
                                    .ReadFrom.Configuration(context.Configuration)
                                    .CreateLogger();

                            loggingBuilder.ClearProviders();
                            loggingBuilder.AddSerilog(logger, dispose: true);
                        });

                        services.AddSingleton<IServiceInfoCreatorService, ServiceInfoCreatorService>();
                        services.AddHostedService<FindRobotService>();
                    })

                    .Build();
            
            
            SetServiceInfo(host.Services);
            await host.RunAsync();

        }

        private static void SetServiceInfo(IServiceProvider serviceProvider)
        {
            IHostEnvironment hostEnvironment = serviceProvider.GetService<IHostEnvironment>();
            ILogger<Program> logger = serviceProvider.GetService<ILogger<Program>>();

            var isDevelopment = hostEnvironment.IsDevelopment();
            if (isDevelopment)
            {
                logger.LogInformation("App run in development mode");

                IServiceInfoCreatorService serviceInfoCreatorService = serviceProvider.GetService<IServiceInfoCreatorService>();
                serviceInfoCreatorService.UpdateOrCreateServiceInfoFile();
            }
        }
    }
}
