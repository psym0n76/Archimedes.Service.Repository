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
        private readonly IHttpRepository _messageClient;
        private readonly IProducer<StrategyMessage> _producer;
        private readonly IHubContext<MarketHub> _context;
        private readonly BatchLog _batchLog = new();

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
            var logId = _batchLog.Start(
                $"{nameof(Consumer_HandleMessage)} {message.Market} {message.TimeFrame} StartDate: {message.StartDate} EndDate: {message.EndDate} {message.Candles.Count} Candle(s)");

            AddCandleToTable(message);
            UpdateMarketMetrics(message);

            //_batchLog.Update(logId,
            //    $"LastCandleMessage: {message.LastCandleMessage()}");

            if (message.TimeFrame != "1Min")
            {
                _batchLog.Update(logId,
                    $"CandleSubscriber Strategy Request EndDate: {message.EndDate} DateRange {message.DateRanges.Max(a => a.EndDate)} {message.Interval}{message.TimeFrame}");
                ProduceStrategyMessage(message);
            }
            else
            {
                _batchLog.Update(logId,
                    $"CandleSubscriber: Strategy Request NOT REQUIRED EndDate: {message.EndDate} DateRange {message.DateRanges.Max(a => a.EndDate)} {message.Interval}{message.TimeFrame}");
            }

            _logger.LogInformation(_batchLog.Print(logId));
        }

        public void Consume(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(cancellationToken);
        }

        private void ProduceStrategyMessage(CandleMessage message)
        {
            var logId = _batchLog.Start(
                $"{nameof(ProduceStrategyMessage)} {message.Id} {message.ExternalMarketId} {message.TimeFrame}");

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

                _logger.LogInformation(_batchLog.Print(logId, "Publish to StrategyRequestQueue"));

                _producer.PublishMessage(strategyMessage, "StrategyRequestQueue", "5000");

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(logId, $"Error returned from CandleSubscriber ", e));
            }
        }

        private async void UpdateMarketMetrics(CandleMessage message)
        {
            var logId = _batchLog.Start(
                $"{nameof(UpdateMarketMetrics)} Id: {message.Id} MarketId: {message.MarketId} ExtId: {message.ExternalMarketId} {message.TimeFrame}");
            try
            {
                var metrics = await _messageClient.GetCandleMetrics(message);

                if (metrics == null)
                {
                    _logger.LogWarning(_batchLog.Print(logId, "WARNING: Missing MarketMetrics"));
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
                    Quantity = metrics.Quantity,
                    ExternalMarketId = message.ExternalMarketId,

                };

                await _messageClient.UpdateMarket(market);

                _logger.LogInformation(_batchLog.Print(logId, "Send MarketMetrics to Hub"));

                await _context.Clients.All.SendAsync("Update", market);

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(logId, $"Error returned from CandleSubscriber ", e));
            }
        }

        private void AddCandleToTable(CandleMessage message)
        {
            var logId = _batchLog.Start(
                $"{nameof(AddCandleToTable)} {message.Market} {message.TimeFrame} StartDate: {message.StartDate} EndDate: {message.EndDate} {message.Candles.Count} Candle(s)");

            try
            {
                if (!message.Candles.Any())
                {
                    _logger.LogWarning(_batchLog.Print(logId, "Candles missing"));
                }

                if (!_messageClient.PostCandles(message.Candles).Result)
                {
                    _logger.LogWarning(_batchLog.Print(logId, "Unable to POST Candles"));
                    return;
                }

                _logger.LogInformation(_batchLog.Print(logId));
            }

            catch (JsonException j)
            {
                _logger.LogError(_batchLog.Print(logId, $"Unable to Parse Candle message ", j));
            }

            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(logId, $"Error returned from CandleSubscriber ", e));
            }
        }
    }
}