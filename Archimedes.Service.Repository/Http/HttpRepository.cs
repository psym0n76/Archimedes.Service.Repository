using System;
using System.Collections.Generic;
using System.Net;
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
    public class HttpRepository : IHttpRepository
    {
        private readonly ILogger<HttpRepository> _logger;
        private readonly HttpClient _client;
        private readonly BatchLog _batchLog = new();
        private string _logId;

        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1

        public HttpRepository(IOptions<Config> config, HttpClient httpClient,
            ILogger<HttpRepository> logger)
        {
            httpClient.BaseAddress = new Uri($"{config.Value.ApiRepositoryUrl}");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
        }

        public async Task DeletePricesOlderThanOneHour()
        {
            var logId = _batchLog.Start( $"{nameof(DeletePricesOlderThanOneHour)}");

            try
            {
                var response = await _client.DeleteAsync($"price/hour");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(logId,
                                $"DELETE Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));
                }

                _logger.LogInformation(_batchLog.Print(logId, "Successfully Deleted"));
            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(logId, $"Error returned from MessageClient", e));
            }
        }

        public async Task<CandleMetricsDto> GetCandleMetrics(CandleMessage message)
        {
            var logId = _batchLog.Start($"{nameof(GetCandleMetrics)} {message.Market} {message.TimeFrame}");

            try
            {
                var response =
                    await _client.GetAsync(
                        $"candle/candle_metrics?market={message.Market}&granularity={message.TimeFrame}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(logId,
                                $"GET Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));


                    return new CandleMetricsDto();
                }

                _logger.LogInformation(_batchLog.Print(logId, $"Returned 1 Record"));
                return await response.Content.ReadAsAsync<CandleMetricsDto>();

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(_logId, $"Error returned from MessageClient", e));
                return new CandleMetricsDto();
            }
        }

        public async Task UpdateMarket(MarketDto market)
        {
            var logId = _batchLog.Start($"{nameof(UpdateMarket)} {market.Name} {market.TimeFrame} Id: {market.Id} ExtId: {market.ExternalMarketId}");

            try
            {
                var payload = new JsonContent(market);
                var response =
                    await _client.PutAsync("market/market_metrics", payload);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(logId,
                                $"PUT Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));

                }

                _logger.LogInformation(_batchLog.Print(logId, $"Updated Market"));

            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(logId, $"Error returned from MessageClient", e));
            }
        }


        public async Task PostCandles(List<CandleDto> candles)
        {
            var logId = _batchLog.Start($"{nameof(PostCandles)} {candles[0].Market} {candles[0].Granularity} {candles.Count} Candle(s)");

            foreach (var candle in candles)
            {
                await PostCandle(candle);
            }
            
            _logger.LogInformation(_batchLog.Print(logId));
        }


        public async Task PostCandle(CandleDto message)
        {
            var logId = _batchLog.Start($"POST {nameof(PostCandle)} {message.Market} {message.Granularity} {message.TimeStamp}");

            try
            {
                var payload = new JsonContent(message);
                var response = await _client.PostAsync("candle", payload); //switched to synchronous as i need wait to get max candle

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsAsync<string>();

                    if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                    {
                        _logger.LogWarning(_batchLog.Print(logId, $"POST FAILED: {errorResponse}"));
                        return;
                    }

                    if (response.RequestMessage != null)
                    {
                        _logger.LogError(_batchLog.Print(logId, $"POST FAILED: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));
                        return;
                    }
                }

                _logger.LogInformation(_batchLog.Print(logId, $"ADDED Candle"));
            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(logId, $"Error returned from MessageClient", e));
            }
        }


        public void PostPrice(PriceMessage message)
        {
            var logId = _batchLog.Start($"{nameof(PostPrice)} {message.Market}");

            try
            {
                if (message.Prices == null)
                {
                    _logger.LogWarning(_batchLog.Print(logId, "Price payload is empty"));
                    return;
                }

                var payload = new JsonContent(message.Prices);
                var response = _client.PostAsync("price", payload).Result;

                if (!response.IsSuccessStatusCode)
                {
                    if (response.RequestMessage != null)

                        _logger.LogWarning(
                            _batchLog.Print(logId,
                                $"POST Failed: {response.ReasonPhrase} from {response.RequestMessage.RequestUri}"));

                }

                _logger.LogInformation(_batchLog.Print(logId, $"ADDED Price"));
            }
            catch (Exception e)
            {
                _logger.LogError(_batchLog.Print(logId, $"Error returned from MessageClient", e));
            }
        }
    }
}
