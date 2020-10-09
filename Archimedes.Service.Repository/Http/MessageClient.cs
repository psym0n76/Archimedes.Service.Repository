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

        public async Task UpdateMarketMetrics(CandleMessage message)
        {
            var metrics = await GetCandleMetrics(message);

            var market = new MarketDto()
            {
                Id = message.MarketId,
                MaxDate = metrics.MaxDate,
                MinDate = metrics.MinDate,
                Quantity = metrics.Quantity
            };

            await UpdateMarket(market);
        }

        public async Task<MarketDto> GetCandleMetrics(CandleMessage message)
        {
            try
            {
                var response =
                    await _client.GetAsync(
                        $"candle/candle_metrics?market={message.Market}&granularity={message.TimeFrame}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to Post {response.ReasonPhrase} from {_client.BaseAddress}candle");
                    return default;
                }

                var market = await response.Content.ReadAsAsync<MarketDto>();

                return market;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
                return default;
            }
        }

        public async Task UpdateMarket(MarketDto market)
        {
            try
            {
                var payload = new JsonContent(market);

                var response =
                    await _client.PutAsync("market/market_metrics",payload);

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


        public async Task Post(CandleMessage message)
        {
            if (message.Candles == null)
            {
                _logger.LogError($"Candle payload is empty");
                return;
            }

            try
            {
                var payload = new JsonContent(message.Candles);
                var response = await _client.PostAsync("candle", payload);
                //update market table
                var candleMetrics =
                    await _client.GetAsync(
                        $"candle/candle_metrics?market={message.Market}&granularity={message.TimeFrame}");


            ;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to Post {response.ReasonPhrase} from {_client.BaseAddress}candle");
                    return;
                }

                _logger.LogInformation(
                    $"Successfully Posted {message.Candles.Count} Candle(s) {response.ReasonPhrase} from {_client.BaseAddress}candle");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
            }
        }

        public async Task Post(PriceMessage message)
        {
            try
            {
                if (message.Prices == null)
                {
                    _logger.LogError($"Price payload is empty");
                    return;
                }

                var payload = new JsonContent(message.Prices);
                var response = await _client.PostAsync("price", payload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to Post {response.ReasonPhrase} from {_client.BaseAddress}price");
                    return;
                }

                _logger.LogInformation(
                    $"Successfully Posted {message.Prices.Count} Price(s) {response.ReasonPhrase} from {_client.BaseAddress}price");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
            }
        }
    }
}
