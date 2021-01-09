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

        public CandleSubscriberService(ICandleSubscriber candleSubscriber, ILogger<CandleSubscriberService> logger)
        {
            _candleSubscriber = candleSubscriber;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Running CandleSubscriberService");

            Task.Run(() =>
            {
                try
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    _logger.LogInformation($"Subscribed to CandleSubscriberService");
                    _candleSubscriber.Consume(stoppingToken);
                }
                catch (OperationCanceledException ox)
                {
                    _logger.LogError($"Cancellation Invoked {ox.Message} \n\nRetry after 5 secs");
                }

                catch (Exception e)
                {
                    _logger.LogError($"Unknown error found in CandleBackgroundService: {e.Message} {e.StackTrace}");
                }

            }, stoppingToken);

            return Task.CompletedTask;
        }
    }
}