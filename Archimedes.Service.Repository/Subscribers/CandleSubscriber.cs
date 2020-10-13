using System;
using System.Threading;
using Archimedes.Library.Message;
using Archimedes.Library.RabbitMq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Archimedes.Service.Repository
{
    public class CandleSubscriber : ICandleSubscriber
    {
        private readonly ILogger<CandleSubscriber> _logger;
        private readonly ICandleConsumer _consumer;
        private readonly IMessageClient _messageClient;

        public CandleSubscriber(ILogger<CandleSubscriber> logger, ICandleConsumer consumer, IMessageClient messageClient)
        {
            _logger = logger;
            _consumer = consumer;
            _messageClient = messageClient;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        public void Consume(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(cancellationToken);
        }

        private void Consumer_HandleMessage(object sender, MessageHandlerEventArgs args)
        {
            var message = JsonConvert.DeserializeObject<CandleMessage>(args.Message);

            _logger.LogInformation($"Received from CandleResponseQueue:: {message}");
            AddCandleToRepository(message);
            UpdateMarketMetrics(message);
        }

        private async void UpdateMarketMetrics(CandleMessage message)
        {
            try
            {
                await _messageClient.UpdateMarketMetrics(message);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to Update Market Metrics message {e.Message} {e.StackTrace}");
            }
        }

        private async void AddCandleToRepository(CandleMessage message)
        {
            try
            {
               await _messageClient.Post(message);
            }

            catch (JsonException j)
            {
                _logger.LogError($"Unable to Parse Candle message {j.Message} {j.StackTrace}");
            }

            catch (Exception e)
            {
                _logger.LogError($"Unable to Add Candle message to API {e.Message} {e.StackTrace}");
            }
        }
    }
}