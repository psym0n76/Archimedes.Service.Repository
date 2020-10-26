using System;
using System.Threading;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Archimedes.Library.RabbitMq;
using Archimedes.Service.Repository.Hubs;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<MarketMetricHub> _context;

        public CandleSubscriber(ILogger<CandleSubscriber> logger, ICandleConsumer consumer, IMessageClient messageClient, IProducer<StrategyMessage> producer, IHubContext<MarketMetricHub> context)
        {
            _logger = logger;
            _consumer = consumer;
            _messageClient = messageClient;
            _producer = producer;
            _context = context;
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
                    Granularity = message.Interval + message.TimeFrame,
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
                var metrics = await _messageClient.GetCandleMetrics(message);

                var market = new MarketDto()
                {
                    Id = message.MarketId,
                    Granularity = message.Interval + message.TimeFrame,
                    TimeFrame = message.TimeFrame,
                    LastUpdated = DateTime.Now,
                    Interval = message.Interval,
                    MaxDate = metrics.MaxDate,
                    MinDate = metrics.MinDate,
                    Quantity = metrics.Quantity
                };

                await _messageClient.UpdateMarketMetrics(market);
                await _context.Clients.All.SendAsync("Update", market);

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