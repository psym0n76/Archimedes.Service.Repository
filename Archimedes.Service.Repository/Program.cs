﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Archimedes.Service.Repository
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Initialise Main");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                logger.Error($"Stopped program because of exception: {e.Message} {e.StackTrace}");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventLog();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.CaptureStartupErrors(false);
                }).UseNLog()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<CandleSubscriberService>();
                    services.AddHostedService<PriceSubscriberService>();
                }); // this ensures we have logging
    }
}
