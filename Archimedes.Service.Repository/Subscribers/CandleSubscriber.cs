using System;
using System.Linq;
using System.Threading;
using Archimedes.Library.Logger;
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
        private readonly ICandleFanoutConsumer _consumer;
        private readonly IMessageClient _messageClient;
        private readonly IProducer<StrategyMessage> _producer;
        private readonly IHubContext<MarketHub> _context;
        private readonly BatchLog _batchLog = new();
        private string _logId;

        public CandleSubscriber(ILogger<CandleSubscriber> logger, ICandleFanoutConsumer consumer,
            IMessageClient messageClient, IProducer<StrategyMessage> producer, IHubContext<MarketHub> context)
        {
            _logger = logger;
            _consumer = consumer;
            _messageClient = messageClient;
            _producer = producer;
            _context = context;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        private void Consumer_HandleMessage(object sender, CandleMessageHandlerEventArgs e)
        {
            _logId = _batchLog.Start();
            var message = e.Message;

            _batchLog.Update(_logId,
                $"Received CandleResponse: {message.Market} {message.Interval}{message.TimeFrame} StartDate:{message.StartDate} EndDate:{message.EndDate} Records:{message.Candles.Count}");
            
            AddCandleToRepository(message);
            UpdateMarketMetrics(message);

            if (e.Message.LastCandleMessage() && e.Message.TimeFrame == "1Min")
            {
                ProduceStrategyMessage(message);
            }
            else
            {
                _batchLog.Update(_logId,
                    $"Received CandleResponse: NO Strategy Request EndDate: {message.EndDate} DateRange {message.DateRanges.Max(a=>a.EndDate)}");
            }
        }

        public void Consume(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(cancellationToken);
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

                _batchLog.Update(_logId, "Publish to StrategyRequestQueue");
                _logger.LogInformation(_batchLog.Print(_logId));

                _producer.PublishMessage(strategyMessage, "StrategyRequestQueue");

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

                if (metrics == null)
                {
                    _batchLog.Update(_logId, "WARNING: Missing MarketMetrics");
                    return;
                }

                var market = new MarketDto()
                {
                    Id = message.MarketId,
                    Name = message.Market,
                    Granularity = message.Interval + message.TimeFrame,
                    TimeFrame = message.TimeFrame,
                    LastUpdated = DateTime.Now,
                    Interval = message.Interval,
                    MaxDate = metrics.MaxDate,
                    MinDate = metrics.MinDate,
                    Quantity = metrics.Quantity
                };

                await _messageClient.UpdateMarketMetrics(market);

                _batchLog.Update(_logId, "Send MarketMetrics to Hub");

                await _context.Clients.All.SendAsync("Update", market);

            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to Update Market Metrics message {e.Message} {e.StackTrace}");
            }
        }

        private void AddCandleToRepository(CandleMessage message)
        {
            try
            {
                _batchLog.Update(_logId, "Post to Repository API");
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