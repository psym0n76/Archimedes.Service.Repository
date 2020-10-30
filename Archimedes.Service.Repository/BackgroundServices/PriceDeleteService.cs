﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class PriceDeleteService : BackgroundService
    {
        private readonly ILogger<CandleSubscriberService> _logger;
        private readonly IMessageClient _client;

        public PriceDeleteService(ILogger<CandleSubscriberService> logger, IMessageClient client)
        {
            _logger = logger;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _client.DeletePricesOlderThanOneHour();
                    await Task.Delay(3600000, stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Error deleting historic prices {e.Message} {e.StackTrace}");
            }
        }
    }
}