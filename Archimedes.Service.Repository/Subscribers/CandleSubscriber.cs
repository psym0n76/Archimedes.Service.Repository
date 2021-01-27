﻿using System;
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
        private readonly IHttpRepository _messageClient;
        private readonly IProducer<StrategyMessage> _producer;
        private readonly IHubContext<MarketHub> _context;
        private readonly BatchLog _batchLog = new();
        private string _logId;

        public CandleSubscriber(ILogger<CandleSubscriber> logger, ICandleFanoutConsumer consumer,
            IHttpRepository messageClient, IProducer<StrategyMessage> producer, IHubContext<MarketHub> context)
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
            var message = e.Message;

            _logId = _batchLog.Start($"CandleSubscriber {message.Market} {message.TimeFrame} StartDate: {message.StartDate} EndDate: {message.EndDate} {message.Candles.Count} Candle(s)");

            AddCandleToTable(message);
            UpdateMarketMetrics(message);

            _batchLog.Update(_logId,
                $"LastCandleMessage: {e.Message.LastCandleMessage()}");

            if (e.Message.LastCandleMessage() && $"{e.Message.TimeFrame}" != "1Min")
            {
                _batchLog.Update(_logId,
                    $"CandleSubscriber Strategy Request EndDate: {message.EndDate} DateRange {message.DateRanges.Max(a => a.EndDate)} {e.Message.Interval}{e.Message.TimeFrame}");
                ProduceStrategyMessage(message);
            }
            else
            {
                _batchLog.Update(_logId,
                    $"CandleSubscriber: Strategy Request NOT REQUIRED EndDate: {message.EndDate} DateRange {message.DateRanges.Max(a=>a.EndDate)} {e.Message.Interval}{e.Message.TimeFrame}");
            }
            
            _logger.LogInformation(_batchLog.Print(_logId));
        }

        public void Consume(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(cancellationToken);
        }

        private void ProduceStrategyMessage(CandleMessage message)
        {
            try
            {
                _batchLog.Update(_logId, "ProduceStrategyMessage");
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

                _producer.PublishMessage(strategyMessage, "StrategyRequestQueue","5000");

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from CandleSubscriber ", e));
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
                    Granularity = message.TimeFrame,
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
                 _logger.LogError(_batchLog.Print(_logId,$"Error returned from CandleSubscriber ",e));
            }
        }

        private void AddCandleToTable(CandleMessage message)
        {
            try
            {
                _batchLog.Update(_logId, $"{nameof(AddCandleToTable)} {message.Market} {message.TimeFrame}");

                if (!message.Candles.Any())
                {
                    _logger.LogWarning(_batchLog.Print(_logId,"Candles missing"));
                }
                
                _messageClient.PostCandles(message.Candles);
                
                _logger.LogInformation(_batchLog.Print(_logId));
            }

            catch (JsonException j)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Unable to Parse Candle message ", j));
            }

            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from CandleSubscriber ", e));
            }
        }
    }
}