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
        private readonly IProducer<StrategyMessage> _producer;

        public CandleSubscriber(ILogger<CandleSubscriber> logger, ICandleConsumer consumer, IMessageClient messageClient, IProducer<StrategyMessage> producer)
        {
            _logger = logger;
            _consumer = consumer;
            _messageClient = messageClient;
            _producer = producer;
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
            ProduceStrategyMessage(message);
        }

        private void ProduceStrategyMessage(CandleMessage message)
        {
            try
            {
                var strategyMessage = new StrategyMessage()
                {
                    Interval = message.Interval,
                    Market = message.Market,
                    Granularity = message.TimeFrame,
                    StartDate = DateTime.Now.AddDays(-3),
                    EndDate = DateTime.Now
                };

                _producer.PublishMessage(strategyMessage,"StrategyRequestQueue");

            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to Add Strategy message to Rabbit{e.Message} {e.StackTrace}");
            }
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

        private  void AddCandleToRepository(CandleMessage message)
        {
            try
            { 
                _messageClient.Post(message);
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