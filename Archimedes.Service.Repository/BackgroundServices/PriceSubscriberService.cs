using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Archimedes.Service.Repository
{
    public class PriceSubscriberService : BackgroundService
    {
        private readonly IPriceSubscriber _priceSubscriber;
        private readonly ILogger<PriceSubscriberService> _logger;

        public PriceSubscriberService(IPriceSubscriber priceSubscriber, ILogger<PriceSubscriberService> logger)
        {
            _priceSubscriber = priceSubscriber;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                try
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    _logger.LogInformation($"Subscribed to PriceSubscriberService");
                    _priceSubscriber.Consume(stoppingToken);
                }
                catch (OperationCanceledException ox)
                {
                    _logger.LogError($"Cancellation Invoked {ox.Message} \n\nRetry after 5 secs");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Unknown error found in PriceBackgroundService {e.Message} {e.StackTrace}");
                }
            }, stoppingToken);

            return Task.CompletedTask;
        }
    }
}