using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sdk.Server.Logging;

namespace RabbitStreamMonitoring
{
    public class Program
    {
        private sealed class RemoteSettingsConfig
        {
            public IReadOnlyCollection<string> RemoteSettingsUrls { get; set; }
        }

        public static void Main(string[] args)
        {
            Console.Title = "Common RabbitStreamMonitoring";

            var remoteSettingsConfig = ApplicationEnvironment.Config.Get<RemoteSettingsConfig>();

            DescribeApplication(remoteSettingsConfig);

            using var loggerFactory = LogConfigurator.Configure("Common", remoteSettingsConfig.RemoteSettingsUrls ?? Array.Empty<string>());
            
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                logger.LogInformation("Application is being started");

                CreateHostBuilder(loggerFactory, remoteSettingsConfig).Build().Run();

                logger.LogInformation("Application has been stopped");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Application has been terminated unexpectedly");
            }
        }

        private static void DescribeApplication(RemoteSettingsConfig remoteSettingsConfig)
        {
            Console.WriteLine($"Application name: {ApplicationInformation.AppName}");
            Console.WriteLine($"Application version: {ApplicationInformation.AppVersion}");

            if (remoteSettingsConfig?.RemoteSettingsUrls != null)
            {
                foreach (var url in remoteSettingsConfig.RemoteSettingsUrls)
                {
                    Console.WriteLine($"Settings url: {url}");
                    var cl = new HttpClient();
                    var s = cl.GetStringAsync(url).Result;
                    Console.WriteLine($"Length: {s.Length}. First: {s.Substring(0,20)}");
                }
            }
            else
            {
                Console.WriteLine("Settings url: none");
            }
        }

        private static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory, RemoteSettingsConfig remoteSettingsConfig) =>
            new HostBuilder()
                .SwisschainService<Startup>(options =>
                {
                    options.UseLoggerFactory(loggerFactory);
                    options.AddWebJsonConfigurationSources(remoteSettingsConfig.RemoteSettingsUrls ?? Array.Empty<string>());
                });
    }
}
