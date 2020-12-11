using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class CandleSubscriberService : BackgroundService
    {
        private readonly ICandleSubscriber _candleSubscriber;
        private readonly ILogger<CandleSubscriberService> _logger;
        private int _counter;

        public CandleSubscriberService(ICandleSubscriber candleSubscriber, ILogger<CandleSubscriberService> logger)
        {
            _candleSubscriber = candleSubscriber;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            if (_counter != 0)
            {
                _logger.LogInformation("Already Subscribed to CandleSubscriberService");
                return Task.CompletedTask;
            }

            _counter++;

            Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation($"Subscribed to CandleSubscriberService {_counter}");
                    _candleSubscriber.Consume(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Unknown error found in CandleBackgroundService: {e.Message} {e.StackTrace}");
                }
            }, stoppingToken);

            _logger.LogWarning("Job cancelled with Token");

            return Task.CompletedTask;
        }
    }
}