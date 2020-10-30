using System;
using System.Net.Http;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Extensions;
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
                var response = await _client.DeleteAsync($"price/hour");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to DELETE {response.ReasonPhrase} from {_client.BaseAddress}price/hour");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to DELETE from {_client.BaseAddress}price/hour {e.Message} {e.StackTrace}");
            }
        }

        public async Task UpdateMarketMetrics(MarketDto message)
        {
            try
            {
                await UpdateMarket(message);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
            }
        }

        public async Task<CandleMetricsDto> GetCandleMetrics(CandleMessage message)
        {
            try
            {
                var response =
                    await _client.GetAsync(
                        $"candle/candle_metrics?market={message.Market}&granularity={message.Interval}{message.TimeFrame}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to Get {response.ReasonPhrase} from {_client.BaseAddress}candle/candle_metrics?market={message.Market}&granularity={message.Interval}{message.TimeFrame}");
                    return default;
                }

                var market = await response.Content.ReadAsAsync<CandleMetricsDto>();

                _logger.LogInformation($"Received CandleMetrics: {market}");

                return market;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
                return default;
            }
        }

        public async Task UpdateMarket(MarketDto metric)
        {
            try
            {
                var payload = new JsonContent(metric);

                var response =
                    await _client.PutAsync("market/market_metrics", payload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to Post {response.ReasonPhrase} from {_client.BaseAddress}candle");
                }

            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
            }
        }

        public void Post(CandleMessage message)
        {
            if (message.Candles == null)
            {
                _logger.LogError($"Candle payload is empty");
                return;
            }

            try
            {
                var payload = new JsonContent(message.Candles);
                var response =  _client.PostAsync("candle", payload).Result; //switched to synchronous as i need wait to get max candle

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to Post {response.ReasonPhrase} from {_client.BaseAddress}candle");
                    return;
                }

                _logger.LogInformation(
                    $"\n\n ADDED {message.Candles.Count} Candle(s) on {message.Market} {message.Interval}{message.TimeFrame}\n");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
            }
        }

        public void Post(PriceMessage message)
        {
            try
            {
                if (message.Prices == null)
                {
                    _logger.LogError($"Price payload is empty");
                    return;
                }

                var payload = new JsonContent(message.Prices);
                var response = _client.PostAsync("price", payload).Result;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to Post {response.ReasonPhrase} from {_client.BaseAddress}price");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
            }
        }
    }
}
