using System;
using System.Net.Http;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Extensions;
using Archimedes.Library.Logger;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Repository
{
    public class MessageClient : IMessageClient
    {
        private readonly ILogger<MessageClient> _logger;
        private readonly HttpClient _client;
        private readonly BatchLog _batchLog = new();
        private string _logId;

        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1

        public MessageClient(IOptions<Config> config, HttpClient httpClient,
            ILogger<MessageClient> logger)
        {
            httpClient.BaseAddress = new Uri($"{config.Value.ApiRepositoryUrl}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
        }

        public async Task DeletePricesOlderThanOneHour()
        {
            try
            {
                _logId = _batchLog.Start();
                _batchLog.Update(_logId, "DELETE DeletePricesOlderThanOneHour");

                var response = await _client.DeleteAsync($"price/hour");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(_logId,
                                $"DELETE Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));
                }

                _logger.LogInformation(_batchLog.Print(_logId, "Successfully Deleted"));
            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from MessageClient", e));
            }
        }

        public async Task UpdateMarketMetrics(MarketDto message)
        {
            try
            {
                _logId = _batchLog.Start();
                _batchLog.Update(_logId,
                    $"UpdateMarketMetrics {message.Name} {message.Granularity} {message.LastUpdated}");
                await UpdateMarket(message);
            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from MessageClient", e));
            }
        }

        public async Task<CandleMetricsDto> GetCandleMetrics(CandleMessage message)
        {
            try
            {
                _logId = _batchLog.Start();
                _batchLog.Update(_logId, $"GET GetCandleMetrics {message.Market} {message.TimeFrame}");

                var response =
                    await _client.GetAsync(
                        $"candle/candle_metrics?market={message.Market}&granularity={message.Interval}{message.TimeFrame}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(_logId,
                                $"GET Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));


                    return new CandleMetricsDto();
                }

                _logger.LogInformation(_batchLog.Print(_logId, $"Returned 1 Record"));
                return await response.Content.ReadAsAsync<CandleMetricsDto>();

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from MessageClient", e));
                return new CandleMetricsDto();
            }
        }

        public async Task UpdateMarket(MarketDto metric)
        {
            try
            {
                _logId = _batchLog.Start();
                _batchLog.Update(_logId, $"GET UpdateMarket {metric.Name} {metric.TimeFrame}");
                var payload = new JsonContent(metric);

                var response =
                    await _client.PutAsync("market/market_metrics", payload);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(_logId,
                                $"PUT Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));

                }

                _logger.LogInformation(_batchLog.Print(_logId, $"Updated Market"));

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from MessageClient", e));
            }
        }

        public void Post(CandleMessage message)
        {
            _logId = _batchLog.Start();
            _batchLog.Update(_logId, $"POST AddCandle {message.Market} {message.TimeFrame}");

            if (message.Candles == null)
            {
                _logger.LogWarning(_batchLog.Print(_logId, "Price payload is empty"));
                return;
            }

            try
            {
                var payload = new JsonContent(message.Candles);
                var response =
                    _client.PostAsync("candle", payload)
                        .Result; //switched to synchronous as i need wait to get max candle

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(_logId,
                                $"POST Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));

                }

                _logger.LogInformation(_batchLog.Print(_logId, $"ADDED Candle"));
            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from MessageClient", e));
            }
        }

        public void Post(PriceMessage message)
        {
            try
            {
                _logId = _batchLog.Start();
                _batchLog.Update(_logId, $"GET GetCandleMetrics {message.Market}");
                
                if (message.Prices == null)
                {
                    _logger.LogWarning(_batchLog.Print(_logId, "Price payload is empty"));
                    return;
                }

                var payload = new JsonContent(message.Prices);
                var response = _client.PostAsync("price", payload).Result;

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(_logId,
                                $"POST Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));


                }

                _logger.LogInformation(_batchLog.Print(_logId, $"ADDED Price"));
            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from MessageClient", e));
            }
        }
    }
}
